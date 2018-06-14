using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Clockwise;
using WorkspaceServer.Models;
using WorkspaceServer.Models.Completion;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Models.SingatureHelp;
using Microsoft.CodeAnalysis;
using WorkspaceServer.Servers.Roslyn;
using System.Linq;
using WorkspaceServer.Transformations;
using WorkspaceServer.Servers.Scripting;
using Microsoft.CodeAnalysis.Recommendations;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis.Completion;
using Pocket;
using Recipes;
using Workspace = WorkspaceServer.Models.Execution.Workspace;
using WorkspaceServer.WorkspaceFeatures;
using static Pocket.Logger<WorkspaceServer.Servers.InMemory.RoslynWorkspaceServer>;

namespace WorkspaceServer.Servers.InMemory
{
    public class RoslynWorkspaceServer : ILanguageService, ICodeRunner
    {
        private readonly WorkspaceRegistry _registry;
        private const int defaultBudgetInSeconds = 30;
        private readonly ConcurrentDictionary<string, AsyncLock> locks = new ConcurrentDictionary<string, AsyncLock>();
        private readonly BufferInliningTransformer _transformer = new BufferInliningTransformer();

        private readonly string UserCodeCompleted = nameof(UserCodeCompleted);

        public RoslynWorkspaceServer(WorkspaceRegistry registry)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        public async Task<CompletionResult> GetCompletionList(WorkspaceRequest request, Budget budget)
        {
            budget = budget ?? new TimeBudget(TimeSpan.FromSeconds(defaultBudgetInSeconds));
            var workspace = await _registry.GetWorkspace(request.Workspace.WorkspaceType);

            var processed = await _transformer.TransformAsync(request.Workspace, budget);
            var sourceFiles = processed.GetSourceFiles();
            var documents = (await workspace.WithSources(sourceFiles, budget)).documents.ToArray();

            var file = processed.GetFileFromBufferId(request.ActiveBufferId);
            var (line, column, absolutePosition) = processed.GetTextLocation(request.ActiveBufferId, request.Position);
            Document selectedDocument = documents.First(doc => doc.Name == file.Name);

            var service = CompletionService.GetService(selectedDocument);
            var completionList = await service.GetCompletionsAsync(selectedDocument, absolutePosition);
            var semanticModel = await selectedDocument.GetSemanticModelAsync();
            var symbols = await Recommender.GetRecommendedSymbolsAtPositionAsync(semanticModel, request.Position, selectedDocument.Project.Solution.Workspace);

            var symbolToSymbolKey = new Dictionary<(string, int), ISymbol>();
            foreach (var symbol in symbols)
            {
                var key = (symbol.Name, (int)symbol.Kind);
                if (!symbolToSymbolKey.ContainsKey(key))
                {
                    symbolToSymbolKey[key] = symbol;
                }
            }

            var completionItems = await Task.WhenAll(
                                      completionList.Items
                                                    .Where(i => i != null)
                                                    .Select(item => item.ToModel(symbolToSymbolKey, selectedDocument)));

            return new CompletionResult(completionItems
                                        .Deduplicate()
                                        .ToArray());
        }

        public async Task<DiagnosticResult> GetDiagnostics(Workspace request, Budget budget)
        {
            budget = budget ?? new TimeBudget(TimeSpan.FromSeconds(defaultBudgetInSeconds));
            var workspace = await _registry.GetWorkspace(request.WorkspaceType);

            var processed = await _transformer.TransformAsync(request, budget);
            var sourceFiles = processed.GetSourceFiles();
            var (compilation, _) = await workspace.WithSources(sourceFiles, budget);

            var diagnostics = await ServiceHelpers.GetDiagnostics(request, compilation);
            return new DiagnosticResult(diagnostics.Select(e => e.Diagnostic).ToArray());
        }

        public async Task<SignatureHelpResponse> GetSignatureHelp(WorkspaceRequest request, Budget budget)
        {
            budget = budget ?? new TimeBudget(TimeSpan.FromSeconds(defaultBudgetInSeconds));

            var workspace = await _registry.GetWorkspace(request.Workspace.WorkspaceType);

            Workspace processed = await _transformer.TransformAsync(request.Workspace, budget);

            var sourceFiles = processed.GetSourceFiles();
            var (compilation, documents) = await workspace.WithSources(sourceFiles, budget);

            var requestActiveBufferId = request.ActiveBufferId.Split("@").First();

            var document = documents.FirstOrDefault(doc => doc.Name == requestActiveBufferId) 
                           ??
                           (documents.Count() == 1 ? documents.Single() : null);

            var tree = await document.GetSyntaxTreeAsync();

            var absolutePosition = processed.GetAbsolutePosition(
                request.ActiveBufferId, 
                request.Position);

            var syntaxNode = tree.GetRoot().FindToken(absolutePosition).Parent;

            return await SignatureHelpService.GetSignatureHelp(
                       () => Task.FromResult(compilation.GetSemanticModel(tree)),
                       syntaxNode,
                       absolutePosition);
        }

        public async Task<RunResult> Run(Workspace workspaceModel, Budget budget = null)
        {
            budget = budget ?? new TimeBudget(TimeSpan.FromSeconds(defaultBudgetInSeconds));
            RunResult runResult = null;

            using (var operation = Log.OnEnterAndConfirmOnExit())
            using (await locks.GetOrAdd(workspaceModel.WorkspaceType, s => new AsyncLock()).LockAsync())
            {
                var workspace = await _registry.GetWorkspace(workspaceModel.WorkspaceType);
                var processed = await _transformer.TransformAsync(workspaceModel, budget);

                var sourceFiles = processed.GetSourceFiles();
                var (compilation, documents) = await workspace.WithSources(sourceFiles, budget);

                var diagnostics = compilation.GetDiagnostics()
                                             .Select(e => new SerializableDiagnostic(e))
                                             .ToArray();

                var d3 = await ServiceHelpers.GetDiagnostics(
                             workspaceModel, compilation);

                if (diagnostics.Any(e => e.Severity == DiagnosticSeverity.Error))
                {
                    return new RunResult(
                        false,
                        d3
                            .Where(d => d.Diagnostic.Severity == DiagnosticSeverity.Error)
                            .Select(d => d.ErrorMessage)
                            .ToArray(),
                        diagnostics: d3.Select(d => d.Diagnostic).ToArray());
                }

                var numberOfAttempts = 100;
                for (var attempt = 1; attempt < numberOfAttempts; attempt++)
                {
                    try
                    {
                        compilation.Emit(workspace.EntryPointAssemblyPath.FullName);
                        operation.Info("Emit succeeded on attempt #{attempt}", attempt);
                        break;
                    }
                    catch (IOException)
                    {
                        if (attempt == numberOfAttempts - 1)
                        {
                            throw;
                        }

                        await Task.Delay(10);
                    }
                }

                string exceptionMessage = null;

                if (workspace.IsWebProject)
                {
                    var webServer = new WebServer(workspace);

                    runResult = new RunResult(
                        succeeded: true,
                        diagnostics: d3.Select(d => d.Diagnostic).ToArray());

                    runResult.AddFeature(webServer);
                }
                else
                {
                    var dotnet = new MLS.Agent.Tools.Dotnet(workspace.Directory);

                    var commandLineResult = await dotnet.Execute(
                                                workspace.EntryPointAssemblyPath.FullName,
                                                budget);

                    budget.RecordEntry(UserCodeCompleted);

                    if (commandLineResult.ExitCode == 124)
                    {
                        throw new BudgetExceededException(budget);
                    }

                    if (commandLineResult.Error.Count > 0)
                    {
                        exceptionMessage = string.Join(Environment.NewLine, commandLineResult.Error);
                    }

                    runResult = new RunResult(
                        succeeded: true,
                        output: commandLineResult?.Output,
                        exception: exceptionMessage,
                        diagnostics: diagnostics,
                        instrumentation: Array.Empty<string>());
                }
            }

            return runResult;
        }
    }
}
