using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Clockwise;
using Microsoft.CodeAnalysis;
using MLS.Agent.Tools;
using Pocket;
using WorkspaceServer.Models;
using WorkspaceServer.Models.Completion;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Models.SingatureHelp;
using WorkspaceServer.Transformations;
using WorkspaceServer.WorkspaceFeatures;
using static Pocket.Logger<WorkspaceServer.Servers.Dotnet.DotnetWorkspaceServer>;
using static WorkspaceServer.Transformations.OmniSharpDiagnosticTransformer;
using Workspace = MLS.Agent.Tools.Workspace;
using OmnisharpEmitResponse = OmniSharp.Client.Commands.OmniSharpResponseMessage<OmniSharp.Client.Commands.EmitResponse>;

namespace WorkspaceServer.Servers.Dotnet
{
    public class DotnetWorkspaceServer : IWorkspaceServer, IDisposable
    {
        private readonly Workspace _workspace;
        private readonly OmniSharpServer _omniSharpServer;
        private readonly AsyncLazy<bool> _initialized;
        private bool _disposed;
        private readonly Budget _initializationBudget = new Budget();
        private readonly TimeSpan _defaultTimeoutInSeconds;
        private readonly BufferInliningTransformer _transformer = new BufferInliningTransformer();
        private ImmutableHashSet<FileInfo> _bufferNameCache = ImmutableHashSet<FileInfo>.Empty;

        public static string UserCodeCompletedBudgetEntryName = "UserCodeCompleted";
        
        public DotnetWorkspaceServer(
            Workspace workspace,
            int? defaultTimeoutInSeconds = 30)
        {
            _workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));

            _defaultTimeoutInSeconds = TimeSpan.FromSeconds(defaultTimeoutInSeconds ?? 30);

            _omniSharpServer = new OmniSharpServer(
                _workspace.Directory,
                Paths.EmitPlugin,
                logToPocketLogger: false);

            _initialized = new AsyncLazy<bool>(async () => await Initialise());
        }

        private async Task<bool> Initialise()
        {
            using (var operation = Log.OnEnterAndConfirmOnExit())
            {
                await _workspace.EnsureBuilt(_initializationBudget);
                await _omniSharpServer.WorkspaceReady(_initializationBudget);
                _initializationBudget.RecordEntry();
                operation.Succeed();
            }

            return true;
        }

        public async Task EnsureInitializedAndNotDisposed(Budget budget = null)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(DotnetWorkspaceServer));
            }

            await _initialized.ValueAsync()
                              .CancelIfExceeds(budget ?? new Budget());
            budget?.RecordEntryAndThrowIfBudgetExceeded();
        }

        public async Task<RunResult> Run(Models.Execution.Workspace workspace, Budget budget = null)
        {
            budget = budget ?? new TimeBudget(_defaultTimeoutInSeconds);

            using (var operation = Log.OnEnterAndConfirmOnExit())
            {
                await EnsureInitializedAndNotDisposed(budget);

                workspace = await _transformer.TransformAsync(workspace, budget);

                string exceptionMessage = null;

                await CleanBuffer(budget);
                var emitResponse = await Emit(workspace, budget);

                var diagnostics = ReconstructDiagnosticLocations(
                    emitResponse.Body.Diagnostics,
                    _transformer.ExtractViewPorts(workspace),
                    BufferInliningTransformer.PaddingSize).ToArray();

                if (emitResponse.Body.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
                {
                    return new RunResult(
                        false,
                        diagnostics
                            .Where(d => d.Diagnostic.Severity == DiagnosticSeverity.Error)
                            .Select(d => d.Message)
                            .ToArray(),
                        diagnostics: diagnostics.Select(d => d.Diagnostic).ToArray());
                }

                RunResult runResult = null;

                if (_workspace.IsWebProject)
                {
                    var webServer = new WebServer(_workspace);

                    runResult = new RunResult(
                        succeeded: true,
                        diagnostics: diagnostics.Select(d => d.Diagnostic).ToArray());

                    runResult.AddFeature(webServer);
                }
                else
                {
                    var dotnet = new MLS.Agent.Tools.Dotnet(_workspace.Directory);

                    var commandLineResult = await dotnet.Execute(
                                                _workspace.EntryPointAssemblyPath.FullName,
                                                budget);

                    budget.RecordEntry(UserCodeCompletedBudgetEntryName);

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
                        diagnostics: diagnostics.Select(d => d.Diagnostic).ToArray());
                }

                operation.Complete(runResult, budget);

                return runResult;
            }
        }

        private async Task CleanBuffer(Budget budget)
        {
            if (_bufferNameCache.Count == 0)
            {
                var wi = await _omniSharpServer.GetWorkspaceInformation(budget);
                var omnisharpFile = wi.Body.MSBuildSolution.Projects.SelectMany(p => p.SourceFiles);

                foreach (var serverBuffer in omnisharpFile)
                {
                    if (Path.GetExtension(serverBuffer.Name).EndsWith("cs", StringComparison.OrdinalIgnoreCase))
                    {
                        var file = new FileInfo(Path.Combine(_workspace.Directory.FullName, serverBuffer.Name));
                        _bufferNameCache = _bufferNameCache.Add(file);
                        await _omniSharpServer.UpdateBuffer(file, "//empty", budget);
                    }
                }
            }

            foreach (var buffer in _bufferNameCache)
            {
                await _omniSharpServer.UpdateBuffer(buffer, "//empty", budget);
            }
            budget.RecordEntry();
        }

        private async Task<OmnisharpEmitResponse> Emit(Models.Execution.Workspace workspace, Budget budget)
        {
            await UpdateOmnisharpWorkspace(workspace);
          
            var emitResponse = await _omniSharpServer.Emit(budget);

            budget.RecordEntryAndThrowIfBudgetExceeded();

            var builtLocation = emitResponse.Body.OutputAssemblyPath;

            if (_workspace.IsWebProject)
            {
                File.Copy(
                    builtLocation,
                    _workspace.EntryPointAssemblyPath.FullName,
                    true);
            }

            return emitResponse;
        }

        private async Task UpdateOmnisharpWorkspace(Models.Execution.Workspace workspace)
        {
            foreach (var sourceFile in workspace.Files)
            {
                var file = new FileInfo(Path.Combine(_workspace.Directory.FullName, sourceFile.Name));
                if (sourceFile.Name.EndsWith(".cs"))
                {
                    _bufferNameCache = _bufferNameCache.Add(file);
                }
                var text = sourceFile.Text.ToString();

                await _omniSharpServer.UpdateBuffer(file, text);
            }
        }
        
        public async Task<CompletionResult> GetCompletionList(WorkspaceRequest request, Budget budget = null)
        {
            budget = budget ?? new TimeBudget(_defaultTimeoutInSeconds);

            using (var operation = Log.OnEnterAndConfirmOnExit())
            {
                await EnsureInitializedAndNotDisposed(budget);
                var (file, code, line, column, absolutePosition) = await TransformWorkspaceAndPreparePositionalRequest(request, budget);
                var wordToComplete = code.GetWordAt(absolutePosition);
                var response = (await _omniSharpServer.GetCompletionList(file, code, wordToComplete, line, column, budget)).ToCompletionResult();
                budget?.RecordEntryAndThrowIfBudgetExceeded();
                operation.Succeed();
                return response;
            }

        }

        public async Task<SignatureHelpResponse> GetSignatureHelp(WorkspaceRequest request, Budget budget = null)
        {
            budget = budget ?? new TimeBudget(_defaultTimeoutInSeconds);

            using (var operation = Log.OnEnterAndConfirmOnExit())
            {
                await EnsureInitializedAndNotDisposed(budget);
                var (file,code,line,column,_) = await TransformWorkspaceAndPreparePositionalRequest(request, budget);
                var response = await _omniSharpServer.GetSignatureHelp(file, code, line, column, budget);
                response = response?.ProcessDocumentation() ?? new SignatureHelpResponse();
                budget?.RecordEntryAndThrowIfBudgetExceeded();
                operation.Succeed();
                return response;
            }
        }

        private async Task<(FileInfo file,string code, int line, int column, int absolutePosition)> TransformWorkspaceAndPreparePositionalRequest(WorkspaceRequest request, Budget budget)
        {
            var workspace = await _transformer.TransformAsync(request.Workspace, budget);
            await CleanBuffer(budget);
            await UpdateOmnisharpWorkspace(workspace);
            var file = workspace.GetFileInfoFromBufferId(request.ActiveBufferId, _workspace.Directory.FullName);
            var code = workspace.GetFileFromBufferId(request.ActiveBufferId).Text;
            // line and colum are 0 based
            var (line,column, absolutePosition) = workspace.GetTextLocation(request.ActiveBufferId, request.Position);
            return (file,code, line, column, absolutePosition);
        }

        public async Task<DiagnosticResult> GetDiagnostics(Models.Execution.Workspace request, Budget budget = null)
        {
            budget = budget ?? new TimeBudget(_defaultTimeoutInSeconds);
          
            using (var operation = Log.OnEnterAndConfirmOnExit())
            {
                await EnsureInitializedAndNotDisposed(budget);
                request = await _transformer.TransformAsync(request, budget);
                var emitResponse = await Emit(request, new Budget());
                SerializableDiagnostic[] diagnostics;
                if (emitResponse.Body.Diagnostics.Any())
                {
                    var viewPorts = _transformer.ExtractViewPorts(request);
                    var processedDiagnostics = ReconstructDiagnosticLocations(emitResponse.Body.Diagnostics, viewPorts, BufferInliningTransformer.PaddingSize).ToArray();
                    diagnostics = processedDiagnostics?.Select(d => d.Diagnostic).ToArray();
                }
                else
                {
                    diagnostics = emitResponse.Body.Diagnostics.Select(d => new SerializableDiagnostic(d)).ToArray();
                }
                var result = new DiagnosticResult(diagnostics);
                budget?.RecordEntryAndThrowIfBudgetExceeded();
                operation.Succeed();
                return result;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _initializationBudget.Cancel();
                _omniSharpServer?.Dispose();
            }
        }
    }
}
