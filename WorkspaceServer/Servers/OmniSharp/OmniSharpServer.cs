using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Subjects;

namespace WorkspaceServer.Servers.OmniSharp
{
    public class OmniSharpServer : IDisposable
    {
        private static readonly Lazy<FileInfo> _omnisharpPath = new Lazy<FileInfo>(OmniSharp.GetPath);

        private readonly Process _process;

        public OmniSharpServer(DirectoryInfo projectDirectory)
        {
            var subject = new Subject<string>();

            StandardOutput = subject;

            _process = CommandLine.StartProcess(
                _omnisharpPath.Value.FullName,
                $"-lsp",
                projectDirectory,
                subject.OnNext);

            StandardInput = _process.StandardInput;
        }

        public IObservable<string> StandardOutput { get; }

        public StreamWriter StandardInput { get; }

        public void Dispose() => _process?.Kill();
    }
}
