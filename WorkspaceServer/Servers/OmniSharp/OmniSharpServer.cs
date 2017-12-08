using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Subjects;
using External;

namespace WorkspaceServer.Servers.OmniSharp
{
    public class OmniSharpServer : IDisposable
    {
        private static readonly Lazy<FileInfo> _omnisharpPath = new Lazy<FileInfo>(OmniSharp.GetPath);

        private readonly Process _process;

        public OmniSharpServer(DirectoryInfo projectDirectory)
        {
            var standardOutput = new ReplaySubject<string>();
            var standardError = new ReplaySubject<string>();

            StandardOutput = standardOutput;
            StandardError = standardError;

            _process = CommandLine.StartProcess(
                _omnisharpPath.Value.FullName,
                $"-lsp",
                projectDirectory,
                standardOutput.OnNext,
                standardError.OnNext);

            StandardInput = _process.StandardInput;
        }

        public IObservable<string> StandardOutput { get; }

        public IObservable<string> StandardError { get; }

        public StreamWriter StandardInput { get; }

        public void Dispose() => _process.KillTree(5000);
    }
}
