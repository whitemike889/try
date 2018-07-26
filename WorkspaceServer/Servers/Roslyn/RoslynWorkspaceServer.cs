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
using WorkspaceServer.Models.SingatureHelp;
using WorkspaceServer.Servers.Roslyn.Instrumentation;
using WorkspaceServer.Servers.Scripting;
using WorkspaceServer.Transformations;
using WorkspaceServer.WorkspaceFeatures;
using static Pocket.Logger<WorkspaceServer.Servers.Roslyn.RoslynWorkspaceServer>;
using Workspace = WorkspaceServer.Models.Execution.Workspace;

namespace WorkspaceServer.Servers.Roslyn
{
    public class RoslynWorkspaceServer : ILanguageService, ICodeRunner
    {
        private readonly GetWorkspaceBuildByName getWorkspaceBuildByName;
        private const int defaultBudgetInSeconds = 30;
        private readonly ConcurrentDictionary<string, AsyncLock> locks = new ConcurrentDictionary<string, AsyncLock>();
        private readonly BufferInliningTransformer _transformer = new BufferInliningTransformer();

        private readonly string UserCodeCompleted = nameof(UserCodeCompleted);

        private delegate Task<WorkspaceBuild> GetWorkspaceBuildByName(string name);

        public RoslynWorkspaceServer(WorkspaceBuild workspaceBuild)
        {
            if (workspaceBuild == null)
            {
                throw new ArgumentNullException(nameof(workspaceBuild));
            }

            getWorkspaceBuildByName = s => Task.FromResult(workspaceBuild);
        }

        public RoslynWorkspaceServer(WorkspaceRegistry registry)
        {
            if (registry == null)
            {
                throw new ArgumentNullException(nameof(registry));
            }

            getWorkspaceBuildByName = s => registry.Get(s);
        }

        public async Task<CompletionResult> GetCompletionList(WorkspaceRequest request, Budget budget)
        {
            budget = budget ?? new TimeBudget(TimeSpan.FromSeconds(defaultBudgetInSeconds));
            var build = await getWorkspaceBuildByName(request.Workspace.WorkspaceType);

            var processed = await _transformer.TransformAsync(request.Workspace, budget);
            var sourceFiles = processed.GetSourceFiles();
            var documents = (await build.GetCompilation(sourceFiles, budget)).documents;

            var file = processed.GetFileFromBufferId(request.ActiveBufferId);
            var (line, column, absolutePosition) = processed.GetTextLocation(request.ActiveBufferId);
            Document selectedDocument = documents.First(doc => doc.Name == file.Name);

            var service = CompletionService.GetService(selectedDocument);
            var completionList = await service.GetCompletionsAsync(selectedDocument, absolutePosition);
            var semanticModel = await selectedDocument.GetSemanticModelAsync();
            var symbols = await Recommender.GetRecommendedSymbolsAtPositionAsync(
                              semanticModel,
                              absolutePosition,
                              selectedDocument.Project.Solution.Workspace);

            var symbolToSymbolKey = new Dictionary<(string, int), ISymbol>();
            foreach (var symbol in symbols)
            {
                var key = (symbol.Name, (int)symbol.Kind);
                if (!symbolToSymbolKey.ContainsKey(key))
                {
                    symbolToSymbolKey[key] = symbol;
                }
            }

            if (completionList == null)
            {
                return new CompletionResult();
            }

            var completionItems = await Task.WhenAll(
                                      completionList.Items
                                                    .Where(i => i != null)
                                                    .Select(item => item.ToModel(symbolToSymbolKey, selectedDocument)));

            return new CompletionResult(completionItems
                                        .Deduplicate()
                                        .ToArray());
        }

        public async Task<SignatureHelpResult> GetSignatureHelp(WorkspaceRequest request, Budget budget)
        {
            budget = budget ?? new TimeBudget(TimeSpan.FromSeconds(defaultBudgetInSeconds));

            var build = await getWorkspaceBuildByName(request.Workspace.WorkspaceType);

            Workspace processed = await _transformer.TransformAsync(request.Workspace, budget);

            var sourceFiles = processed.GetSourceFiles();
            var (compilation, documents) = await build.GetCompilation(sourceFiles, budget);

            var requestActiveBufferId = request.ActiveBufferId.Split("@").First();

            var document = documents.FirstOrDefault(doc => doc.Name == requestActiveBufferId)
                           ??
                           (documents.Count == 1 ? documents.Single() : null);

            if (document == null)
            {
                return new SignatureHelpResult();
            }

            var tree = await document.GetSyntaxTreeAsync();

            var absolutePosition = processed.GetAbsolutePositionForGetBufferWithSpecifiedIdOrSingleBufferIfThereIsOnlyOne(request.ActiveBufferId);

            var syntaxNode = tree.GetRoot().FindToken(absolutePosition).Parent;

            return await SignatureHelpService.GetSignatureHelp(
                       () => Task.FromResult(compilation.GetSemanticModel(tree)),
                       syntaxNode,
                       absolutePosition);
        }

        public async Task<RunResult> Run(Workspace workspace, Budget budget = null)
        {
            budget = budget ?? new TimeBudget(TimeSpan.FromSeconds(defaultBudgetInSeconds));
            RunResult runResult = null;

            using (var operation = Log.OnEnterAndExit())
            using (await locks.GetOrAdd(workspace.WorkspaceType, s => new AsyncLock()).LockAsync())
            {
                WorkspaceBuild build;
                using (Log.OnEnterAndConfirmOnExit("ConfigureWorkspace"))
                {
                    build = await getWorkspaceBuildByName(workspace.WorkspaceType);
                    workspace = await _transformer.TransformAsync(workspace, budget);
                }

                var compilation = await Compile(workspace, budget, build);
                var diagnostics = ServiceHelpers.GetDiagnostics(
                    workspace,
                    compilation);

                if (diagnostics.Any(e => e.Severity == DiagnosticSeverity.Error))
                {
                    var compileErrorMessages = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)
                                                                   .Select(d => d.Message)
                                                                   .ToArray();
                    return new RunResult(
                        false,
                        compileErrorMessages,
                        diagnostics: diagnostics);
                }

                await EmitCompilation(compilation, build);

                if (build.IsWebProject)
                {
                    runResult = RunWebRequest(build);
                }
                else if (build.IsUnitTestProject)
                {
                    runResult = await RunDotnetTest(budget, build, diagnostics);
                }
                else
                {
                    runResult = await RunDotnetConsoleApp(workspace, budget, runResult, build, diagnostics);
                }
            }

            return runResult;
        }

        private async Task<RunResult> RunDotnetConsoleApp(Workspace workspace, Budget budget, RunResult runResult, WorkspaceBuild build, SerializableDiagnostic[] diagnostics)
        {
            using (Log.OnEnterAndExit())
            {
                var dotnet = new Dotnet(build.Directory);

                var commandLineResult = await dotnet.Execute(
                                            build.EntryPointAssemblyPath.FullName,
                                            budget);

                var output = InstrumentedOutputExtractor.ExtractOutput(commandLineResult.Output);

                budget.RecordEntry(UserCodeCompleted);

                if (commandLineResult.ExitCode == 124)
                {
                    throw new BudgetExceededException(budget);
                }

                string exceptionMessage = null;

                if (commandLineResult.Error.Count > 0)
                {
                    exceptionMessage = string.Join(Environment.NewLine, commandLineResult.Error);
                }

                runResult = new RunResult(
                       succeeded: true,
                       output: output.StdOut,
                       exception: exceptionMessage,
                       diagnostics: diagnostics);

                if (workspace.IncludeInstrumentation)
                {
                    runResult.AddFeature(output.ProgramStatesArray);
                    runResult.AddFeature(output.ProgramDescriptor);
                }

                return runResult;
            }
        }

        private static async Task<RunResult> RunDotnetTest(Budget budget, WorkspaceBuild build, SerializableDiagnostic[] diagnostics)
        {
            using (Log.OnEnterAndExit())
            {
                var dotnet = new Dotnet(build.Directory);

                var testRunResult = await dotnet.VSTest(
                                        $"--logger:trx {build.EntryPointAssemblyPath}",
                                        budget);

                string exceptionMessage = null;

                if (testRunResult.Error.Count > 0)
                {
                    exceptionMessage = string.Join(Environment.NewLine, testRunResult.Error);
                }

                var trex = new FileInfo(
                    Path.Combine(
                        Paths.DotnetToolsPath,
                        "t-rex".ExecutableName()));

                CommandLineResult tRexResult = null;

                if (trex.Exists)
                {
                    tRexResult = await CommandLine.Execute(
                                     trex,
                                     "",
                                     workingDir: build.Directory,
                                     budget: budget);
                }

                var result = new RunResult(
                    testRunResult.ExitCode == 0,
                    tRexResult?.Output ?? testRunResult.Output,
                    exceptionMessage,
                    diagnostics);

                result.AddFeature(new UnitTestRun(new[]
                {
                        new UnitTestResult()
                    }));

                return result;
            }
        }

        private async Task<Compilation> Compile(Workspace workspace, Budget budget, WorkspaceBuild build)
        {
            using (Log.OnEnterAndExit())
            {
                var sourceFiles = workspace.GetSourceFiles();

                var (compilation, documents) = await build.GetCompilation(sourceFiles, budget);

                var viewports = _transformer.ExtractViewPorts(workspace);
                var instrumentationRegions = viewports.Where(v => v.Destination?.Name != null)
                                                      .GroupBy(v => v.Destination.Name, v => v.Region, (name, regions) => new InstrumentationMap(name, regions));

                if (workspace.IncludeInstrumentation)
                {
                    compilation = AugmentCompilation(instrumentationRegions, compilation, documents.First().Project.Solution);
                }

                return compilation;
            }
        }

        private static RunResult RunWebRequest(WorkspaceBuild build)
        {
            using (Log.OnEnterAndExit())
            {
                var runResult = new RunResult(succeeded: true);
                runResult.AddFeature(new WebServer(build));
                return runResult;
            }
        }

        private static async Task EmitCompilation(Compilation compilation, WorkspaceBuild build)
        {
            using (var operation = Log.OnEnterAndExit())
            {
                var numberOfAttempts = 100;
                for (var attempt = 1; attempt < numberOfAttempts; attempt++)
                {
                    try
                    {
                        compilation.Emit(build.EntryPointAssemblyPath.FullName);
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
            }

        }

        private Compilation AugmentCompilation(IEnumerable<InstrumentationMap> regions, Compilation compilation, Solution solution)
        {
            using (Log.OnEnterAndExit())
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
}
