using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Clockwise;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using MLS.Agent.Tools;
using Pocket;
using WorkspaceServer.Models;
using WorkspaceServer.Models.Completion;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Models.SingatureHelp;
using WorkspaceServer.Transformations;
using WorkspaceServer.WorkspaceFeatures;
using static Pocket.Logger<WorkspaceServer.Servers.Dotnet.DotnetWorkspaceServer>;
using static WorkspaceServer.Servers.WorkspaceServer;
using static WorkspaceServer.Transformations.OmniSharpDiagnosticTransformer;
using Workspace = MLS.Agent.Tools.Workspace;
using OmnisharpEmitResponse = OmniSharp.Client.Commands.OmniSharpResponseMessage<OmniSharp.Client.Commands.EmitResponse>;
using OmniSharp = MLS.Agent.Tools.OmniSharp;
using TextSpan = Microsoft.CodeAnalysis.Text.TextSpan;

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

            _initialized = new AsyncLazy<bool>(async () =>
            {
                await _workspace.EnsureBuilt(_initializationBudget);
                await _omniSharpServer.WorkspaceReady(_initializationBudget);
                return true;
            });
        }

        public async Task EnsureInitializedAndNotDisposed(Budget budget = null)
        {
            budget?.RecordEntryAndThrowIfBudgetExceeded();

            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(DotnetWorkspaceServer));
            }

            await _initialized.ValueAsync()
                              .CancelIfExceeds(budget ?? new Budget());
        }

        public async Task<RunResult> Run(Models.Execution.Workspace workspace, Budget budget = null)
        {
            budget = budget ?? new TimeBudget(_defaultTimeoutInSeconds);

            using (var operation = Log.OnEnterAndConfirmOnExit())
            {
                await EnsureInitializedAndNotDisposed(budget);

                workspace = await _transformer.TransformAsync(workspace, budget);

                string exceptionMessage = null;

                await FlushBuffers(budget);

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

        private async Task FlushBuffers(Budget budget)
        {
            var wi = await _omniSharpServer.GetWorkspaceInformation(budget);
            var omnisharpFile = wi.Body.MSBuildSolution.Projects.SelectMany(p => p.SourceFiles);
           
            foreach (var serverBuffer in omnisharpFile)
            {
                if (Path.GetExtension(serverBuffer.Name).EndsWith("cs", StringComparison.OrdinalIgnoreCase))
                {
                    var file = new FileInfo(Path.Combine(_workspace.Directory.FullName, serverBuffer.Name));
                    await _omniSharpServer.UpdateBuffer(file, "//empty",budget);
                }
            }

            budget.RecordEntry();
        }

        private async Task<OmnisharpEmitResponse> Emit(Models.Execution.Workspace request, Budget budget)
        {
            await UpdateOmnisharpWorkspace(request);

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

                var text = sourceFile.Text;

                await _omniSharpServer.UpdateBuffer(file, text);
            }
        }

        public async Task<CompletionResult> GetCompletionList(CompletionRequest request, Budget budget = null)
        {
            budget = budget ?? new TimeBudget(_defaultTimeoutInSeconds);

            using (var operation = Log.OnEnterAndConfirmOnExit())
            {
                await EnsureInitializedAndNotDisposed(budget);
                var processor = new BufferInliningTransformer();
                var workspaceProcessing = processor.TransformAsync(request.Workspace, budget);
                await FlushBuffers(budget);
                var workspace = await workspaceProcessing;
                await UpdateOmnisharpWorkspace(workspace);
                var fileName = workspace.GetFileFromBufferId(request.ActiveBufferId)?.Name;
                var location = workspace.GetLocation(request.ActiveBufferId, request.Position);

                throw new NotImplementedException();
            }
        }

        public async Task<SignatureHelpResponse> GetSignatureHelp(SignatureHelpRequest request, Budget budget = null)
        {
            budget = budget ?? new TimeBudget(_defaultTimeoutInSeconds);

            using (var operation = Log.OnEnterAndConfirmOnExit())
            {
                await EnsureInitializedAndNotDisposed(budget);
                var processor = new BufferInliningTransformer();
                var workspaceProcessing =  processor.TransformAsync(request.Workspace, budget);
                await FlushBuffers(budget);
                var workspace = await workspaceProcessing;
                await UpdateOmnisharpWorkspace(workspace);
                var fileName = workspace.GetFileFromBufferId(request.ActiveBufferId)?.Name;
                var location = workspace.GetLocation(request.ActiveBufferId, request.Position);

                budget.RecordEntry();
                return new SignatureHelpResponse();
            }
        }

        public async Task<DiagnosticResult> GetDiagnostics(Models.Execution.Workspace request, Budget budget = null)
        {
            budget = budget ?? new TimeBudget(_defaultTimeoutInSeconds);
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

            return new DiagnosticResult(diagnostics);
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
