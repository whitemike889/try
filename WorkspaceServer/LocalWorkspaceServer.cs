using System;
using System.IO;
using System.Threading.Tasks;

namespace WorkspaceServer
{
    public class LocalWorkspaceServer : IWorkspaceServer
    {
        private readonly Dotnet _dotnet;

        private readonly DirectoryInfo _workingDirectory;

        public LocalWorkspaceServer(DirectoryInfo workingDirectory, Dotnet dotnet = null)
        {
            _workingDirectory = workingDirectory ?? throw new ArgumentNullException(nameof(workingDirectory));

            _dotnet = dotnet ?? new Dotnet(_workingDirectory);
        }

        public async Task<ProcessResult> CompileAndExecute(BuildAndRunRequest request)
        {
            EnsureWorkingDirectoryExists();

            CleanOldSources();

            _dotnet.New("console");

            new FileInfo(Path.Combine(_workingDirectory.FullName, "Program.cs")).Delete();

            WriteUserSourceFiles(request.Sources);

            _dotnet.Restore();

            return _dotnet.Run();
        }

        private void CleanOldSources()
        {
            foreach (var sourceFile in _workingDirectory.EnumerateFileSystemInfos("*.cs"))
            {
                sourceFile.Delete();
            }
        }

        private void EnsureWorkingDirectoryExists()
        {
            if (!_workingDirectory.Exists)
            {
                _workingDirectory.Create();
            }
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
