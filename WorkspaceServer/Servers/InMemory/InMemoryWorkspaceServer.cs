using System;
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
using System.Collections.Immutable;
using System.IO;
using Microsoft.CodeAnalysis.Completion;
using MLS.Agent.Tools;
using Pocket;
using Recipes;
using Workspace = WorkspaceServer.Models.Execution.Workspace;
using WorkspaceServer.WorkspaceFeatures;
using static Pocket.Logger<WorkspaceServer.Servers.InMemory.InMemoryWorkspaceServer>;

namespace WorkspaceServer.Servers.InMemory
{
    public class InMemoryWorkspaceServer : ILanguageService, ICodeRunner
    {
        private const int defaultBudgetInSeconds = 30;
        private readonly ImmutableDictionary<string, AsyncLock> locks;
        private readonly ImmutableDictionary<string, AsyncLazy<InMemoryWorkspace>> workspacesCache;
        private readonly BufferInliningTransformer _transformer = new BufferInliningTransformer();

        private readonly string UserCodeCompleted = nameof(UserCodeCompleted);

        public InMemoryWorkspaceServer(DotnetWorkspaceServerRegistry registry)
        {
            if (registry == null)
            {
                throw new ArgumentNullException(nameof(registry));
            }

            workspacesCache = new Dictionary<string, AsyncLazy<InMemoryWorkspace>>
            {
                ["console"] = new AsyncLazy<InMemoryWorkspace>(async () => new InMemoryWorkspace(
                                                                   "console",
                                                                   await registry.GetWorkspace("console"),
                                                                   WorkspaceUtilities.DefaultReferencedAssemblies)),

                ["script"] = new AsyncLazy<InMemoryWorkspace>(async () => new InMemoryWorkspace(
                                                                  "script",
                                                                  await registry.GetWorkspace("console"),
                                                                  WorkspaceUtilities.DefaultReferencedAssemblies)),

                ["nodatime.api"] = new AsyncLazy<InMemoryWorkspace>(async () => new InMemoryWorkspace(
                                                                        "nodatime.api",
                                                                        await registry.GetWorkspace("nodatime.api"),
                                                                        WorkspaceUtilities.DefaultReferencedAssemblies)),
            }.ToImmutableDictionary();

            locks = workspacesCache.ToImmutableDictionary(p => p.Key, _ => new AsyncLock());
        }

        public async Task<CompletionResult> GetCompletionList(WorkspaceRequest request, Budget budget)
        {
            budget = budget ?? new TimeBudget(TimeSpan.FromSeconds(defaultBudgetInSeconds));
            var workspace = await workspacesCache[request.Workspace.WorkspaceType].ValueAsync();

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
            var workspace = await workspacesCache[request.WorkspaceType].ValueAsync();

            var processed = await _transformer.TransformAsync(request, budget);
            var sourceFiles = processed.GetSourceFiles();
            var (compilation, _) = await workspace.WithSources(sourceFiles, budget);

            var diagnostics = await ServiceHelpers.GetDiagnostics(request, compilation);
            return new DiagnosticResult(diagnostics.Select(e => e.Diagnostic).ToArray());
        }

        public async Task<SignatureHelpResponse> GetSignatureHelp(WorkspaceRequest request, Budget budget)
        {
            budget = budget ?? new TimeBudget(TimeSpan.FromSeconds(defaultBudgetInSeconds));

            var workspace = await workspacesCache[request.Workspace.WorkspaceType].ValueAsync();

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
            using (await locks[workspaceModel.WorkspaceType].LockAsync())
            {
                var workspace = await workspacesCache[workspaceModel.WorkspaceType].ValueAsync();
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
                        compilation.Emit(workspace.Workspace.EntryPointAssemblyPath.FullName);
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

                if (workspace.Workspace.IsWebProject)
                {
                    var webServer = new WebServer(workspace.Workspace);

                    runResult = new RunResult(
                        succeeded: true,
                        diagnostics: d3.Select(d => d.Diagnostic).ToArray());

                    runResult.AddFeature(webServer);
                }
                else
                {
                    var dotnet = new MLS.Agent.Tools.Dotnet(workspace.Workspace.Directory);

                    var commandLineResult = await dotnet.Execute(
                                                workspace.Workspace.EntryPointAssemblyPath.FullName,
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
