using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Subjects;

namespace WorkspaceServer.Servers.OmniSharp
{
    public class OmniSharp : IDisposable
    {
        private readonly string projectDirectory;
        private readonly string _omnisharpPath = @"C:\dev\github\omnisharp-roslyn\artifacts\publish\OmniSharp.Stdio\win7-x64\OmniSharp.exe";

        private readonly ISubject<string> subject;
        private Process _process;

        public OmniSharp(string projectDirectory)
        {
            this.projectDirectory = projectDirectory;
            subject = new ReplaySubject<string>();
            StandardOut = subject;
            Start();
            StandardInput = _process.StandardInput;
        }

        public IObservable<string> StandardOut { get; }
        public StreamWriter StandardInput { get; private set; }

        private void Start()
        {
            _process = CommandLine.StartProcess(_omnisharpPath, null, projectDirectory, data => subject.OnNext(data));
        }

        public void Dispose()
        {
            _process?.Kill();
        }
    }
}
