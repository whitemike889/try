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
using Microsoft.CodeAnalysis.Completion;
using MLS.Agent.Tools;
using Workspace = WorkspaceServer.Models.Execution.Workspace;
using WorkspaceServer.WorkspaceFeatures;

namespace WorkspaceServer.Servers.InMemory
{
    public class InMemoryWorkspaceServer : ILanguageService, ICodeRunner
    {
        private const int defaultTimeSpanInSeconds = 30;
        private readonly ConcurrentDictionary<string, AsyncLazy<InMemoryWorkspace>> workspacesCache;
        private readonly BufferInliningTransformer _transformer = new BufferInliningTransformer();

        private readonly string UserCodeCompleted = nameof(UserCodeCompleted);

        public InMemoryWorkspaceServer(DotnetWorkspaceServerRegistry registry)
        {
            if (registry == null)
            {
                throw new ArgumentNullException(nameof(registry));
            }

            workspacesCache = new ConcurrentDictionary<string, AsyncLazy<InMemoryWorkspace>>
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
            };
        }

        public async Task<CompletionResult> GetCompletionList(WorkspaceRequest request, Budget budget)
        {
            budget = budget ?? new TimeBudget(TimeSpan.FromSeconds(defaultTimeSpanInSeconds));
            var workspace = await workspacesCache[request.Workspace.WorkspaceType].ValueAsync();

            var processed = await _transformer.TransformAsync(request.Workspace, budget);
            var viewPorts = _transformer.ExtractViewPorts(processed);
            var sourceFiles = processed.GetSourceFiles();
            var (compilation, documents) = await workspace.WithSources(sourceFiles, budget);

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
                                                    .Select(item => item.ToModel(symbolToSymbolKey, selectedDocument)));

            return new CompletionResult(completionItems
                                        .Deduplicate()
                                        .ToArray());
        }

        public async Task<DiagnosticResult> GetDiagnostics(Workspace request, Budget budget)
        {
            budget = budget ?? new TimeBudget(TimeSpan.FromSeconds(defaultTimeSpanInSeconds));
            var workspace = await workspacesCache[request.WorkspaceType].ValueAsync();

            var processed = await _transformer.TransformAsync(request, budget);
            var viewPorts = _transformer.ExtractViewPorts(processed);
            var sourceFiles = processed.GetSourceFiles();
            var (compilation, _) = await workspace.WithSources(sourceFiles, budget);

            var diagnostics = await ServiceHelpers.GetDiagnostics(request, compilation);
            return new DiagnosticResult(diagnostics.Select(e => e.Diagnostic).ToArray());
        }

        public async Task<SignatureHelpResponse> GetSignatureHelp(WorkspaceRequest request, Budget budget)
        {
            budget = budget ?? new TimeBudget(TimeSpan.FromSeconds(defaultTimeSpanInSeconds));
            var (code, line, column, _) = await TransformWorkspaceAndPreparePositionalRequest(request, budget);

            var workspace = await workspacesCache[request.Workspace.WorkspaceType].ValueAsync();

            Workspace processed = await _transformer.TransformAsync(request.Workspace, budget);

            var viewPorts = _transformer.ExtractViewPorts(processed);
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

        public  async Task<RunResult> Run(Workspace workspaceModel, Budget budget = null)
        {
            budget = budget ?? new TimeBudget(TimeSpan.FromSeconds(defaultTimeSpanInSeconds));

            var workspace = await workspacesCache[workspaceModel.WorkspaceType].ValueAsync();
            var processed = await _transformer.TransformAsync(workspaceModel, budget);
            var viewPorts = _transformer.ExtractViewPorts(processed);
            var sourceFiles = processed.GetSourceFiles();
            var (compilation, documents) = await workspace.WithSources(sourceFiles, budget);

            var diagnostics = compilation.GetDiagnostics()
                                        .Select(e => new SerializableDiagnostic(e))
                                        .ToArray();

            var d3 = await ServiceHelpers.GetDiagnostics(
                workspaceModel, compilation);

            //var diagnostics2 = DiagnosticTransformer.ReconstructDiagnosticLocations(
            //    diagnostics
            //    _transformer.ExtractViewPorts(workspaceModel),
            //    BufferInliningTransformer.PaddingSize
            //).ToArray();

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

            compilation.Emit(workspace.Workspace.EntryPointAssemblyPath.FullName);

            RunResult runResult = null;
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

            return runResult;
        }

        private async Task<(string code, int line, int column, int absolutePosition)> TransformWorkspaceAndPreparePositionalRequest(WorkspaceRequest request, Budget budget)
        {
            var workspace = await _transformer.TransformAsync(request.Workspace, budget);

            var code = workspace.GetFileFromBufferId(request.ActiveBufferId).Text;
            // line and colum are 0 based

            var (line, column, absolutePosition) = workspace.GetTextLocation(request.ActiveBufferId, request.Position);

            return (code, line, column, absolutePosition);
        }
    }
}
