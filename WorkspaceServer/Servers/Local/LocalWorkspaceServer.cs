using System;
using System.IO;
using System.Threading.Tasks;
using WorkspaceServer.Models.Completion;
using WorkspaceServer.Models.Execution;

namespace WorkspaceServer.Servers.Local
{
    public class LocalWorkspaceServer : IWorkspaceServer
    {
        private readonly DirectoryInfo _workingDirectory;

        public LocalWorkspaceServer(DirectoryInfo workingDirectory)
        {
            _workingDirectory = workingDirectory ?? throw new ArgumentNullException(nameof(workingDirectory));
        }

        public Task<RunResult> Run(RunRequest request)
        {
            var sourceFiles = request.GetSourceFiles();

            var dotnet = new Dotnet(GetWorkingDirectory());

            WriteUserSourceFiles(sourceFiles);

            return Task.FromResult(dotnet.Run());
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
            foreach (var sourceFile in _workingDirectory.EnumerateFileSystemInfos("*.cs"))
            {
                sourceFile.Delete();
            }
        }

        private DirectoryInfo GetWorkingDirectory()
        {
            _workingDirectory.Refresh();

            if (!_workingDirectory.Exists)
            {
                _workingDirectory.Create();

                var dotnet = new Dotnet(_workingDirectory);

                AcquireTemplate(dotnet);

                PrepareWorkspace(dotnet);
            }
            else
            {
                CleanOldSources();
            }

            return _workingDirectory;
        }

        private void WriteUserSourceFiles(SourceFile[] sourceFiles)
        {
            int i = 1;

            foreach (var sourceFile in sourceFiles)
            {
                var filePath = Path.Combine(_workingDirectory.FullName, $"{i++}.cs");
                var text = sourceFile.Text.ToString();

                File.WriteAllText(filePath, text);
            }
        }

        public Task<CompletionResult> GetCompletionList(CompletionRequest request)
        {
            throw new NotSupportedException();
        }
    }
}
