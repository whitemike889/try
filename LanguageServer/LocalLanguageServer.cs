using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace LanguageServer
{
    public class LocalLanguageServer : ILanguageServer
    {
        private readonly Dotnet _dotnet;

        private readonly DirectoryInfo _workingDirectory;

        public LocalLanguageServer(DirectoryInfo workingDirectory, Dotnet dotnet = null)
        {
            _workingDirectory = workingDirectory ?? throw new ArgumentNullException(nameof(workingDirectory));

            _dotnet = dotnet ?? new Dotnet(_workingDirectory);
        }

        public async Task<ProcessResult> CompileAndExecute(CompileAndExecuteRequest request)
        {
            EnsureWorkingDirectoryExists();

            _dotnet.New("console");

            new FileInfo(Path.Combine(_workingDirectory.FullName, "Program.cs")).Delete();

            WriteUserSourceFiles(request.Sources);

            _dotnet.Restore();

            return _dotnet.Run();
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