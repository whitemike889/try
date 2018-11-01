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
using MLS.Protocol.Completion;
using Pocket;
using Recipes;
using WorkspaceServer.Models.Execution;
using MLS.Protocol.SignatureHelp;
using WorkspaceServer.Servers.Roslyn.Instrumentation;
using WorkspaceServer.Servers.Scripting;
using WorkspaceServer.Transformations;
using WorkspaceServer.WorkspaceFeatures;
using WorkspaceServer.Workspaces;
using static Pocket.Logger<WorkspaceServer.Servers.Roslyn.RoslynWorkspaceServer>;
using Workspace = MLS.Protocol.Execution.Workspace;
using MLS.Protocol.Execution;
using MLS.Protocol;
using MLS.Protocol.Extensions;
using MLS.Protocol.Transformations;

namespace WorkspaceServer.Servers.Roslyn
{
    public class RoslynWorkspaceServer : ILanguageService, ICodeRunner, ICodeCompiler
    {
        private readonly GetWorkspaceBuildByName getWorkspaceBuildByName;
        private const int defaultBudgetInSeconds = 30;
        private readonly ConcurrentDictionary<string, AsyncLock> locks = new ConcurrentDictionary<string, AsyncLock>();
        private readonly BufferInliningTransformer _transformer = new BufferInliningTransformer();

        private static readonly string UserCodeCompleted = nameof(UserCodeCompleted);

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
            var (compilation, documents) = await build.GetCompilation(sourceFiles, budget);

            var file = processed.GetFileFromBufferId(request.ActiveBufferId);
            var (line, column, absolutePosition) = processed.GetTextLocation(request.ActiveBufferId);
            var selectedDocument = documents.First(doc => doc.Name == file.Name);

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
                return new CompletionResult(requestId: request.RequestId);
            }

            var completionItems = completionList.Items
                .Where(i => i != null)
                .Select(item => item.ToModel(symbolToSymbolKey, selectedDocument));

            return new CompletionResult(completionItems
                                        .Deduplicate()
                                        .ToArray(),
                requestId: request.RequestId);
        }

        public async Task<SignatureHelpResult> GetSignatureHelp(WorkspaceRequest request, Budget budget)
        {
            budget = budget ?? new TimeBudget(TimeSpan.FromSeconds(defaultBudgetInSeconds));

            var build = await getWorkspaceBuildByName(request.Workspace.WorkspaceType);

            var processed = await _transformer.TransformAsync(request.Workspace, budget);

            var sourceFiles = processed.GetSourceFiles();
            var (compilation, documents) = await build.GetCompilation(sourceFiles, budget);

            var document = documents.FirstOrDefault(doc => doc.Name == request.ActiveBufferId.FileName)
                           ??
                           (documents.Count == 1 ? documents.Single() : null);

            if (document == null)
            {
                return new SignatureHelpResult(requestId: request.RequestId);
            }

            var tree = await document.GetSyntaxTreeAsync();

            var absolutePosition = processed.GetAbsolutePositionForGetBufferWithSpecifiedIdOrSingleBufferIfThereIsOnlyOne(request.ActiveBufferId);

            var syntaxNode = tree.GetRoot().FindToken(absolutePosition).Parent;

            var result = await SignatureHelpService.GetSignatureHelp(
                       () => Task.FromResult(compilation.GetSemanticModel(tree)),
                       syntaxNode,
                       absolutePosition);
            result.RequestId = request.RequestId;
            return result;
        }

        public async Task<CompileResult> Compile(WorkspaceRequest request, Budget budget = null)
        {
            var workspace = request.Workspace;
            budget = budget ?? new TimeBudget(TimeSpan.FromSeconds(defaultBudgetInSeconds));

            using (Log.OnEnterAndExit())
            using (await locks.GetOrAdd(workspace.WorkspaceType, s => new AsyncLock()).LockAsync())
            {
                var (compilation, diagnostics) = await CompileWorker(request.Workspace, request.ActiveBufferId, budget);

                if (diagnostics.ContainsError())
                {
                    return new CompileResult(
                        succeeded: false,
                        base64assembly: null,
                        diagnostics,
                        requestId: request.RequestId);
                }

                using (var stream = new MemoryStream())
                {
                    compilation.Emit(peStream: stream);
                    var encodedAssembly = System.Convert.ToBase64String(stream.ToArray());
                    return new CompileResult(
                        succeeded: true,
                        base64assembly: encodedAssembly,
                        requestId: request.RequestId);
                }
            }
        }

        public async Task<RunResult> Run(WorkspaceRequest request, Budget budget = null)
        {
            var workspace = request.Workspace;
            budget = budget ?? new TimeBudget(TimeSpan.FromSeconds(defaultBudgetInSeconds));

            using (Log.OnEnterAndExit())
            using (await locks.GetOrAdd(workspace.WorkspaceType, s => new AsyncLock()).LockAsync())
            {
                var build = await getWorkspaceBuildByName(workspace.WorkspaceType);

                workspace = await _transformer.TransformAsync(workspace, budget);

                var (compilation, diagnostics) = await CompileWorker(request.Workspace, request.ActiveBufferId, budget);

                if (diagnostics.ContainsError())
                {
                    var compileErrorMessages = diagnostics.GetCompileErrorMessages();
                    return new RunResult(
                        false,
                        compileErrorMessages,
                        diagnostics: diagnostics,
                        requestId: request.RequestId);
                }

                await EmitCompilationAsync(compilation, build);

                if (build.IsWebProject)
                {
                    return RunWebRequest(build, request.RequestId);
                }

                if (build.IsUnitTestProject)
                {
                    return await RunUnitTestsAsync(build, diagnostics, budget, request.RequestId);
                }

                return await RunConsoleAsync(workspace, build, diagnostics, budget, request.RequestId);
            }
        }

        private static async Task EmitCompilationAsync(Compilation compilation, WorkspaceBuild build)
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

        private static async Task<RunResult> RunConsoleAsync(Workspace workspace, WorkspaceBuild build, SerializableDiagnostic[] diagnostics, Budget budget, string requestId)
        {
            var dotnet = new Dotnet(build.Directory);

            var commandLineResult = await dotnet.Execute(
                                        build.EntryPointAssemblyPath.FullName,
                                        budget);

            budget.RecordEntry(UserCodeCompleted);

            var output = InstrumentedOutputExtractor.ExtractOutput(commandLineResult.Output);

            if (commandLineResult.ExitCode == 124)
            {
                throw new BudgetExceededException(budget);
            }

            string exceptionMessage = null;

            if (commandLineResult.Error.Count > 0)
            {
                exceptionMessage = string.Join(Environment.NewLine, commandLineResult.Error);
            }

            var runResult = new RunResult(
                succeeded: true,
                output: output.StdOut,
                exception: exceptionMessage,
                diagnostics: diagnostics,
                requestId: requestId);

            if (workspace.IncludeInstrumentation)
            {
                runResult.AddFeature(output.ProgramStatesArray);
                runResult.AddFeature(output.ProgramDescriptor);
            }

            return runResult;
        }

        private static async Task<RunResult> RunUnitTestsAsync(WorkspaceBuild build, SerializableDiagnostic[] diagnostics, Budget budget, string requestId)
        {
            var dotnet = new Dotnet(build.Directory);

            var commandLineResult = await dotnet.VSTest(
                                        $"--logger:trx {build.EntryPointAssemblyPath}",
                                        budget);

            budget.RecordEntry(UserCodeCompleted);

            if (commandLineResult.ExitCode == 124)
            {
                throw new BudgetExceededException(budget);
            }


            var trex = new FileInfo(
                Path.Combine(
                    Paths.DotnetToolsPath,
                    "t-rex".ExecutableName()));

            if (!trex.Exists)
            {
                throw new InvalidOperationException($"t-rex not found in at location {trex}");
            }

            var tRexResult = await CommandLine.Execute(
                 trex,
                 "",
                 workingDir: build.Directory,
                 budget: budget);

            var result = new RunResult(
                commandLineResult.ExitCode == 0,
                tRexResult.Output,
                diagnostics: diagnostics,
                requestId: requestId);

            result.AddFeature(new UnitTestRun(new[]
            {
                new UnitTestResult()
            }));

            return result;
        }

        private static RunResult RunWebRequest(WorkspaceBuild build, string requestId)
        {
            var runResult = new RunResult(succeeded: true, requestId: requestId);
            runResult.AddFeature(new WebServer(build));
            return runResult;
        }

        private async Task<(Compilation, SerializableDiagnostic[])> CompileWorker(Workspace workspace, BufferId activeBufferId, Budget budget)
        {
            var build = await getWorkspaceBuildByName(workspace.WorkspaceType);
            workspace = await _transformer.TransformAsync(workspace, budget);
            var compilation = await build.Compile(workspace, budget, activeBufferId);
            var diagnostics = workspace.MapDiagnostics(activeBufferId, compilation);
            return (compilation, diagnostics);
        }
    }
}
