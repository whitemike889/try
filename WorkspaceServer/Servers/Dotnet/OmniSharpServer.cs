using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Clockwise;
using MLS.Agent.Tools;
using OmniSharp.Client;
using OmniSharp.Client.Events;
using Pocket;

namespace WorkspaceServer.Servers.Dotnet
{
    public class OmniSharpServer : IDisposable
    {
        private static readonly Lazy<FileInfo> _omniSharpPath = new Lazy<FileInfo>(MLS.Agent.Tools.OmniSharp.GetPath);

        private bool _ready;
        private readonly CompositeDisposable disposables = new CompositeDisposable();
        private int _seq;
        private readonly Lazy<Process> _process;

        public OmniSharpServer(
            DirectoryInfo projectDirectory,
            string pluginPath = null,
            bool logToPocketLogger = false)
        {
            var standardOutput = new ReplaySubject<string>();
            var standardError = new ReplaySubject<string>();

            StandardOutput = standardOutput;
            StandardError = standardError;

            if (logToPocketLogger)
            {
                disposables.Add(StandardOutput.Subscribe(e => Logger<OmniSharpServer>.Log.Info("{message}", args: e)));
                disposables.Add(StandardError.Subscribe(e => Logger<OmniSharpServer>.Log.Error("{message}", args: e)));
            }

            _process = new Lazy<Process>(() =>
            {
                var process =
                    CommandLine.StartProcess(
                        _omniSharpPath.Value.FullName,
                        string.IsNullOrWhiteSpace(pluginPath)
                            ? ""
                            : $"-pl {pluginPath}",
                        projectDirectory,
                        standardOutput.OnNext,
                        standardError.OnNext);

                disposables.Add(() =>
                {
                    if (!process.HasExited)
                    {
                        process.Kill();
                    }
                });
                disposables.Add(process);

                return process;
            });
        }

        public IObservable<string> StandardOutput { get; }

        public IObservable<string> StandardError { get; }

        public StreamWriter StandardInput => _process.Value.StandardInput;

        public void Dispose() => disposables.Dispose();

        public int NextSeq() => Interlocked.Increment(ref _seq);

        public async Task WorkspaceReady(TimeBudget budget = null)
        {
            if (_ready)
            {
                return;
            }

            var _ = _process.Value;

            using (var operation = Logger<OmniSharpServer>.Log.OnEnterAndConfirmOnExit())
            {
                await StandardOutput
                      .AsOmniSharpMessages()
                      .OfType<OmniSharpEventMessage<ProjectAdded>>()
                      .FirstAsync()
                      .ToTask()
                      .CancelIfExceeds(budget ?? TimeBudget.Unlimited());

                _ready = true;

                operation.Succeed();
            }
        }
    }
}
