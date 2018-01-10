using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MLS.Agent.Tools;
using WorkspaceServer.Models.Completion;
using WorkspaceServer.Models.Execution;

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

            var emitResponse = await _omniSharpServer.Emit(timeout);

            if (emitResponse.Body.Errors.Any())
            {
                return new RunResult(
                    false,
                    emitResponse.Body.Errors.Select(e => e.ToString()).ToArray());
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
                exception: exceptionMessage);
        }

        public async Task<CompletionResult> GetCompletionList(CompletionRequest request)
        {
            throw new NotImplementedException();
        }

        public void Dispose() => _omniSharpServer.Dispose();
    }
}
