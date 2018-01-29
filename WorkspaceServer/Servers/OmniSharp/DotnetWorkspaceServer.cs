using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using MLS.Agent.Tools;
using WorkspaceServer.Models.Completion;
using WorkspaceServer.Models.Execution;
using Workspace = MLS.Agent.Tools.Workspace;
using OmnisharpEmitResponse = OmniSharp.Client.Commands.OmniSharpResponseMessage<OmniSharp.Client.Commands.EmitResponse>;

namespace WorkspaceServer.Servers.OmniSharp
{
    public class DotnetWorkspaceServer : IWorkspaceServer, IDisposable
    {
        private readonly TimeSpan _defaultTimeout;
        private readonly Workspace _workspace;
        private readonly OmniSharpServer _omniSharpServer;

        private bool _disposed;

        public DotnetWorkspaceServer(Workspace workspace, int defaultTimeoutInSeconds = WorkspaceServer.DefaultTimeoutInSeconds)
        {
            _defaultTimeout = TimeSpan.FromSeconds(defaultTimeoutInSeconds);

            _workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));

            _omniSharpServer = new OmniSharpServer(
                _workspace.Directory,
                Paths.EmitPlugin,
                true);
        }

        public async Task EnsureInitializedAndNotDisposed(TimeSpan? timeout = null)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(DotnetWorkspaceServer));
            }

            await _workspace.EnsureCreated();

            _workspace.EnsureBuilt();

            await _omniSharpServer.WorkspaceReady(timeout);
        }

        public async Task<RunResult> Run(RunRequest request, TimeSpan? timeout = null)
        {
            await EnsureInitializedAndNotDisposed();

            var emitResponse = await Emit(request, timeout);

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

            var result = dotnet.Execute(emitResponse.Body.OutputAssemblyPath, timeout ?? _defaultTimeout);

            string exceptionMessage = null;

            if (result.Exception != null)
            {
                exceptionMessage = result.Exception.ToString();
            }
            else if (result.Error.Count > 0)
            {
                exceptionMessage = string.Join(Environment.NewLine, result.Error);
            }

            return new RunResult(
                succeeded: !(result.Exception is TimeoutException),
                output: result.Output,
                exception: exceptionMessage,
                diagnostics: emitResponse.Body.Diagnostics.Select(d => new SerializableDiagnostic(d)).ToArray());
        }

        private async Task<OmnisharpEmitResponse> Emit(RunRequest request, TimeSpan? timeout)
        {
            await EnsureInitializedAndNotDisposed();

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

            return await _omniSharpServer.Emit(timeout);
        }

        public Task<CompletionResult> GetCompletionList(CompletionRequest request)
        {
            throw new NotImplementedException();
        }

        public async Task<DiagnosticResult> GetDiagnostics(RunRequest request)
        {
            var emitResult = await Emit(request, timeout: null);
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
