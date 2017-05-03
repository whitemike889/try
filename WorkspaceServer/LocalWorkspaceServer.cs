using System;
using System.IO;
using System.Threading.Tasks;

namespace WorkspaceServer
{
    public class LocalWorkspaceServer : IWorkspaceServer
    {
        private readonly DirectoryInfo _workingDirectory;

        public LocalWorkspaceServer(DirectoryInfo workingDirectory)
        {
            _workingDirectory = workingDirectory ?? throw new ArgumentNullException(nameof(workingDirectory));
        }

        public async Task<ProcessResult> CompileAndExecute(BuildAndRunRequest request)
        {
            var dotnet = new Dotnet(GetWorkingDirectory());

            WriteUserSourceFiles(request.Sources);

            return dotnet.Run();
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

        private void WriteUserSourceFiles(string[] requestSources)
        {
            int i = 1;

            foreach (var requestSource in requestSources)
            {
                File.WriteAllText(Path.Combine(_workingDirectory.FullName, $"{i++}.cs"), requestSource);
            }
        }
    }
}
