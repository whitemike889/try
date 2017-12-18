using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Subjects;
using MLS.Agent.Tools.External;

namespace WorkspaceServer.Servers.OmniSharp
{
    public class OmniSharpServer : IDisposable
    {
        private static readonly Lazy<FileInfo> _omnisharpPath = new Lazy<FileInfo>(OmniSharp.GetPath);

        public OmniSharpServer(DirectoryInfo projectDirectory)
        {
            var standardOutput = new ReplaySubject<string>();
            var standardError = new ReplaySubject<string>();

            StandardOutput = standardOutput;
            StandardError = standardError;

            Process = CommandLine.StartProcess(
                _omnisharpPath.Value.FullName,
                $"-lsp",
                projectDirectory,
                standardOutput.OnNext,
                standardError.OnNext);

            StandardInput = Process.StandardInput;
        }

        public IObservable<string> StandardOutput { get; }

        public IObservable<string> StandardError { get; }

        public StreamWriter StandardInput { get; }

        public Process Process { get; }

        public void Dispose() => Process.KillTree(5000);
    }
}
