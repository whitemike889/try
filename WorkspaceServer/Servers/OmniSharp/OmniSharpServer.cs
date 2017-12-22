using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Subjects;
using System.Threading;
using MLS.Agent.Tools;
using Pocket;
using static Pocket.Logger<WorkspaceServer.Servers.OmniSharp.OmniSharpServer>;

namespace WorkspaceServer.Servers.OmniSharp
{
    public class OmniSharpServer : IDisposable
    {
        private static readonly Lazy<FileInfo> _omnisharpPath = new Lazy<FileInfo>(MLS.Agent.Tools.OmniSharp.GetPath);

        private readonly CompositeDisposable disposables = new CompositeDisposable();
        private int seq;

        public OmniSharpServer(
            DirectoryInfo projectDirectory,
            string pluginPath = null,
            bool logToPocketLogger = false)
        {
            var standardOutput = new Subject<string>();
            var standardError = new Subject<string>();

            StandardOutput = standardOutput;
            StandardError = standardError;

            Process = CommandLine.StartProcess(
                _omnisharpPath.Value.FullName,
                string.IsNullOrWhiteSpace(pluginPath)
                    ? ""
                    : $"-pl {pluginPath}",
                projectDirectory,
                standardOutput.OnNext,
                standardError.OnNext);

            disposables.Add(() => Process.Kill());

            if (logToPocketLogger)
            {
                disposables.Add(StandardOutput.Subscribe(e => Log.Info("{message}", args: e)));
                disposables.Add(StandardError.Subscribe(e => Log.Error("{message}", args: e)));
            }

            StandardInput = Process.StandardInput;
        }

        public IObservable<string> StandardOutput { get; }

        public IObservable<string> StandardError { get; }

        public StreamWriter StandardInput { get; }

        public Process Process { get; }

        public void Dispose() => disposables.Dispose();

        public int NextSeq() => Interlocked.Increment(ref seq);
    }
}
