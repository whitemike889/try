using System;
using System.IO;
using System.Threading.Tasks;
using MLS.Agent.Tools;
using Pocket;
using WorkspaceServer.Models.Completion;
using WorkspaceServer.Models.Execution;
using static Pocket.Logger<WorkspaceServer.Servers.Local.LocalWorkspaceServer>;

namespace WorkspaceServer.Servers.Local
{
    public class LocalWorkspaceServer : IWorkspaceServer
    {
        private readonly DirectoryInfo _workingDirectory;

        public LocalWorkspaceServer(DirectoryInfo workingDirectory)
        {
            _workingDirectory = workingDirectory ??
                                throw new ArgumentNullException(nameof(workingDirectory));

            Log.Trace("Creating workspace in @ {directory}", _workingDirectory);
        }

        public async Task<RunResult> Run(RunRequest request, TimeSpan? timeout = null)
        {
            using (var operation = Log.OnEnterAndConfirmOnExit())
            {
                var sourceFiles = request.GetSourceFiles();

                var dotnet = new Dotnet(GetWorkingDirectory());

                WriteUserSourceFiles(sourceFiles);

                var result = await Task.Run(() => dotnet.Run(timeout));

                operation.Succeed();

                return new RunResult(
                    succeeded: result.ExitCode == 0,
                    output: result.Output,
                    exception: result?.Exception.ToString());
            }
        }

        private static void PrepareWorkspace(Dotnet dotnet)
        {
            dotnet.Restore();
        }

        private void AcquireTemplate(Dotnet dotnet)
        {
            dotnet.New("console");
            new FileInfo(Path.Combine(_workingDirectory.FullName, "Program.cs")).Delete();
        }

        private void CleanOldSources()
        {
            using (Log.OnEnterAndExit())
            {
                foreach (var sourceFile in _workingDirectory.EnumerateFileSystemInfos("*.cs"))
                {
                    sourceFile.Delete();
                }
            }
        }

        private void CreateWorkingDirectory()
        {
            using (Log.OnEnterAndExit())
            {
                _workingDirectory.Create();

                var dotnet = new Dotnet(_workingDirectory);

                AcquireTemplate(dotnet);

                PrepareWorkspace(dotnet);
            }
        }

        private DirectoryInfo GetWorkingDirectory()
        {
            _workingDirectory.Refresh();

            if (!_workingDirectory.Exists)
            {
                CreateWorkingDirectory();
            }
            else
            {
                CleanOldSources();
            }

            return _workingDirectory;
        }

        private void WriteUserSourceFiles(SourceFile[] sourceFiles)
        {
            using (Log.OnEnterAndExit())
            {
                int i = 1;

                foreach (var sourceFile in sourceFiles)
                {
                    var filePath = Path.Combine(_workingDirectory.FullName, $"{i++}.cs");
                    var text = sourceFile.Text.ToString();

                    File.WriteAllText(filePath, text);
                }
            }
        }

        public Task<CompletionResult> GetCompletionList(CompletionRequest request)
        {
            throw new NotSupportedException();
        }
    }
}
