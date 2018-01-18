using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using MLS.Agent.Tools;
using WorkspaceServer.Models.Completion;
using WorkspaceServer.Models.Execution;
using Workspace = MLS.Agent.Tools.Workspace;
using OmnisharpEmitResponse = global::OmniSharp.Client.Commands.OmniSharpResponseMessage<OmniSharp.Client.Commands.EmitResponse>;

namespace WorkspaceServer.Servers.OmniSharp
{
    public class DotnetWorkspaceServer : IWorkspaceServer, IDisposable
    {
        private readonly Workspace workspace;
        private readonly OmniSharpServer _omniSharpServer;

        public DotnetWorkspaceServer(Workspace workspace)
        {
            this.workspace = workspace;

            this.workspace.EnsureCreated("console");

            this.workspace.EnsureBuilt();

            _omniSharpServer = new OmniSharpServer(
                this.workspace.Directory,
                Paths.EmitPlugin,
                true);
        }

        public async Task<RunResult> Run(RunRequest request, TimeSpan? timeout = null)
        {
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

            var dotnet = new Dotnet(workspace.Directory, timeout);

            var result = dotnet.Execute(emitResponse.Body.OutputAssemblyPath);

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
            await _omniSharpServer.WorkspaceReady();

            foreach (var sourceFile in request.SourceFiles)
            {
                var file = new FileInfo(Path.Combine(workspace.Directory.FullName, sourceFile.Name));

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

        public void Dispose() => _omniSharpServer.Dispose();
        public async Task<DiagnosticResult> GetDiagnostics(RunRequest request)
        {
            var emitResult = await Emit(request, timeout: null);
            var diagnostics = emitResult.Body.Diagnostics.Select(d => new SerializableDiagnostic(d)).ToArray();
            return new DiagnosticResult(diagnostics);
        }

    }
}
