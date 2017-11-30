using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Subjects;

namespace WorkspaceServer.Servers.OmniSharp
{
    public class OmniSharpServer : IDisposable
    {
        private readonly FileInfo _omnisharpPath = new FileInfo(@"C:\dev\github\omnisharp-roslyn\artifacts\publish\OmniSharp.Stdio\win7-x64\OmniSharp.exe");

        private readonly Process _process;

        public OmniSharpServer(DirectoryInfo projectDirectory)
        {
            var subject = new ReplaySubject<string>();

            StandardOutput = subject;

            _process = CommandLine.StartProcess(
                _omnisharpPath,
                null,
                projectDirectory,
                subject.OnNext);

            StandardInput = _process.StandardInput;
        }

        public IObservable<string> StandardOutput { get; }

        public StreamWriter StandardInput { get; }

        public void Dispose() => _process?.Kill();
    }
}
