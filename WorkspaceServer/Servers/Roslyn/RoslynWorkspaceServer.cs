using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Clockwise;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Recommendations;
using MLS.Agent.Tools;
using Pocket;
using Recipes;
using WorkspaceServer.Models;
using WorkspaceServer.Models.Completion;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Models.Instrumentation;
using WorkspaceServer.Models.SingatureHelp;
using WorkspaceServer.Servers.Roslyn.Instrumentation;
using WorkspaceServer.Servers.Scripting;
using WorkspaceServer.Transformations;
using WorkspaceServer.WorkspaceFeatures;
using Workspace = WorkspaceServer.Models.Execution.Workspace;

namespace WorkspaceServer.Servers.Roslyn
{
    public class RoslynWorkspaceServer : ILanguageService, ICodeRunner
    {
        private readonly GetWorkspaceByName _getWorkspaceByName;
        private const int defaultBudgetInSeconds = 30;
        private readonly ConcurrentDictionary<string, AsyncLock> locks = new ConcurrentDictionary<string, AsyncLock>();
        private readonly BufferInliningTransformer _transformer = new BufferInliningTransformer();

        private readonly string UserCodeCompleted = nameof(UserCodeCompleted);

        private delegate Task<MLS.Agent.Tools.Workspace> GetWorkspaceByName(string name);

        public RoslynWorkspaceServer(MLS.Agent.Tools.Workspace workspace)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            _getWorkspaceByName = s => Task.FromResult(workspace);
        }

        public RoslynWorkspaceServer(WorkspaceRegistry registry)
        {
            if (registry == null)
            {
                throw new ArgumentNullException(nameof(registry));
            }

            _getWorkspaceByName = s => registry.GetWorkspace(s);
        }

        public async Task<CompletionResult> GetCompletionList(WorkspaceRequest request, Budget budget)
        {
            budget = budget ?? new TimeBudget(TimeSpan.FromSeconds(defaultBudgetInSeconds));
            var workspace = await _getWorkspaceByName(request.Workspace.WorkspaceType);

            var processed = await _transformer.TransformAsync(request.Workspace, budget);
            var sourceFiles = processed.GetSourceFiles();
            var documents = (await workspace.GetCompilation(sourceFiles, budget)).documents;

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
            var workspace = await _getWorkspaceByName(request.WorkspaceType);

            var processed = await _transformer.TransformAsync(request, budget);
            var sourceFiles = processed.GetSourceFiles();
            var (compilation, _) = await workspace.GetCompilation(sourceFiles, budget);

            var diagnostics = await ServiceHelpers.GetDiagnostics(request, compilation);
            return new DiagnosticResult(diagnostics.Select(e => e.Diagnostic).ToArray());
        }

        public async Task<SignatureHelpResponse> GetSignatureHelp(WorkspaceRequest request, Budget budget)
        {
            budget = budget ?? new TimeBudget(TimeSpan.FromSeconds(defaultBudgetInSeconds));

            var workspace = await _getWorkspaceByName(request.Workspace.WorkspaceType);

            Workspace processed = await _transformer.TransformAsync(request.Workspace, budget);

            var sourceFiles = processed.GetSourceFiles();
            var (compilation, documents) = await workspace.GetCompilation(sourceFiles, budget);

            var requestActiveBufferId = request.ActiveBufferId.Split("@").First();

            var document = documents.FirstOrDefault(doc => doc.Name == requestActiveBufferId)
                           ??
                           (documents.Count == 1 ? documents.Single() : null);

            if (document == null)
            {
                return new SignatureHelpResponse();
            }

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

            using (var operation = Logger<RoslynWorkspaceServer>.Log.OnEnterAndConfirmOnExit())
            using (await locks.GetOrAdd(workspaceModel.WorkspaceType, s => new AsyncLock()).LockAsync())
            {
                var workspace = await _getWorkspaceByName(workspaceModel.WorkspaceType);
                var processed = await _transformer.TransformAsync(workspaceModel, budget);

                var sourceFiles = processed.GetSourceFiles();
                var (compilation, documents) = await workspace.GetCompilation(sourceFiles, budget);

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

                var viewports = _transformer.ExtractViewPorts(workspaceModel);
                var instrumentationRegions = viewports.Values
                    .Where(v => v.Destination != null && v.Destination.Name != null)
                    .GroupBy(v => v.Destination.Name, v => v.Region, (name, regions) => new InstrumentationMap(name, regions));

                if (workspaceModel.IncludeInstrumentation)
                {
                    compilation = AugmentCompilation(instrumentationRegions, compilation, documents.First().Project.Solution);
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

                    var output = InstrumentedOutputExtractor.ExtractOutput(commandLineResult.Output);

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
                           output: output.StdOut,
                           exception: exceptionMessage,
                           diagnostics: diagnostics);

                    if (workspaceModel.IncludeInstrumentation)
                    {
                        runResult.AddFeature(output.ProgramStatesArray);
                        runResult.AddFeature(output.ProgramDescriptor);
                    }
                }
            }

            return runResult;
        }

        private Compilation AugmentCompilation(IEnumerable<InstrumentationMap> regions, Compilation compilation, Solution solution)
        {
            var newCompilation = compilation;
            foreach (var tree in newCompilation.SyntaxTrees)
            {
                var replacementRegions = regions?.Where(r => tree.FilePath.EndsWith(r.FileToInstrument)).FirstOrDefault()?.InstrumentationRegions;

                var semanticModel = newCompilation.GetSemanticModel(tree);

                var visitor = new InstrumentationSyntaxVisitor(solution.GetDocument(tree), replacementRegions);
                var linesWithInstrumentation = visitor.Augmentations.Data.Keys;

                var rewrite = new InstrumentationSyntaxRewriter(
                    linesWithInstrumentation,
                    new[] { visitor.VariableLocations },
                    new[] { visitor.Augmentations });
                var newRoot = rewrite.Visit(tree.GetRoot());
                var newTree = tree.WithRootAndOptions(newRoot, tree.Options);

                newCompilation = newCompilation.ReplaceSyntaxTree(tree, newTree);
            }

            // if it failed to compile, just return the original, unaugmented compilation
            var augmentedDiagnostics = newCompilation.GetDiagnostics();
            if (augmentedDiagnostics.Any(e => e.Severity == DiagnosticSeverity.Error))
            {
                throw new Exception("Augmented source failed to compile: " + string.Join(Environment.NewLine, augmentedDiagnostics));
            }

            return newCompilation;
        }
    }
}
