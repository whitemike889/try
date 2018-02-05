using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Clockwise;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;
using MLS.Agent.Tools;
using WorkspaceServer.Models.Completion;
using WorkspaceServer.Models.Execution;
using Workspace = MLS.Agent.Tools.Workspace;
using OmnisharpEmitResponse = OmniSharp.Client.Commands.OmniSharpResponseMessage<OmniSharp.Client.Commands.EmitResponse>;

namespace WorkspaceServer.Servers.OmniSharp
{
    public class DotnetWorkspaceServer : IWorkspaceServer, IDisposable
    {
        private readonly Workspace _workspace;
        private readonly OmniSharpServer _omniSharpServer;

        private bool _disposed;

        public DotnetWorkspaceServer(Workspace workspace)
        {
            _workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));

            _omniSharpServer = new OmniSharpServer(
                _workspace.Directory,
                Paths.EmitPlugin,
                true);
        }

        public async Task EnsureInitializedAndNotDisposed(TimeBudget budget = null)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(DotnetWorkspaceServer));
            }

            budget?.RecordEntryAndThrowIfBudgetExceeded();

            await _workspace.EnsureCreated(budget);

            _workspace.EnsureBuilt(budget);

            await _omniSharpServer.WorkspaceReady(budget);
        }

        public async Task<RunResult> Run(RunRequest request, TimeBudget budget = null)
        {
            budget = budget ?? TimeBudget.Unlimited();

            CommandLineResult result = null;
            Exception exception = null;
            string exceptionMessage = null;
            OmnisharpEmitResponse emitResponse = null;

            try
            {
                await EnsureInitializedAndNotDisposed(budget);

                emitResponse = await Emit(request, budget);

                if (emitResponse.Body.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
                {
                    return new RunResult(
                        false,
                        emitResponse.Body
                                    .Diagnostics
                                    .Where(d => d.Severity == DiagnosticSeverity.Error)
                                    .Select(e => e.ToString())
                                    .ToArray(),
                        diagnostics: emitResponse.Body.Diagnostics.Select(d => new SerializableDiagnostic(d)).ToArray());
                }

                var dotnet = new Dotnet(_workspace.Directory);

                result = dotnet.Execute(emitResponse.Body.OutputAssemblyPath, budget);

                if (result.Exception != null)
                {
                    exceptionMessage = result.Exception.ToString();
                }
                else if (result.Error.Count > 0)
                {
                    exceptionMessage = string.Join(Environment.NewLine, result.Error);
                }
            }
            catch (TimeoutException timeoutException)
            {
                exception = timeoutException;
            }
            catch (TimeBudgetExceededException)
            {
                exception = new TimeoutException(); 
            }
            catch (TaskCanceledException taskCanceledException)
            {
                exception = taskCanceledException;
            }

            return new RunResult(
                succeeded:  !(exception is TimeoutException) &&
                            !(exception is CompilationErrorException),
                output: result?.Output,
                exception: exceptionMessage ?? exception.ToDisplayString(),
                diagnostics: emitResponse?.Body.Diagnostics.Select(d => new SerializableDiagnostic(d)).ToArray());
        }

        private async Task<OmnisharpEmitResponse> Emit(RunRequest request, TimeBudget budget = null)
        {
            await EnsureInitializedAndNotDisposed(budget);

            foreach (var sourceFile in request.SourceFiles)
            {
                var file = new FileInfo(Path.Combine(_workspace.Directory.FullName, sourceFile.Name));

                var text = sourceFile.Text.ToString();

                if (!file.Exists)
                {
                    File.WriteAllText(file.FullName, text);
                }

                await _omniSharpServer.UpdateBuffer(file, text);
            }

            return await _omniSharpServer.Emit(budget);
        }

        public Task<CompletionResult> GetCompletionList(CompletionRequest request)
        {
            throw new NotImplementedException();
        }

        public async Task<DiagnosticResult> GetDiagnostics(RunRequest request)
        {
            var emitResult = await Emit(request);
            var diagnostics = emitResult.Body.Diagnostics.Select(d => new SerializableDiagnostic(d)).ToArray();
            return new DiagnosticResult(diagnostics);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _omniSharpServer?.Dispose();
            }
        }
    }
}
