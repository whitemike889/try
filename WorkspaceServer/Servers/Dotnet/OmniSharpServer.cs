using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Clockwise;
using MLS.Agent.Tools;
using OmniSharp.Client;
using OmniSharp.Client.Events;
using Pocket;
using static Pocket.Logger<WorkspaceServer.Servers.Dotnet.OmniSharpServer>;

namespace WorkspaceServer.Servers.Dotnet
{
    public class OmniSharpServer : IDisposable
    {
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private int _seq;
        private readonly ReplaySubject<string> _standardOutput = new ReplaySubject<string>();
        private readonly ReplaySubject<string> _standardError = new ReplaySubject<string>();
        private readonly AsyncLazy<Process> _omnisharpProcess;

        public OmniSharpServer(
            DirectoryInfo projectDirectory,
            string pluginPath = null,
            bool logToPocketLogger = false)
        {
            if (logToPocketLogger)
            {
                _disposables.Add(StandardOutput.Subscribe(e => Log.Info("{message}", args: e)));
                _disposables.Add(StandardError.Subscribe(e => Log.Error("{message}", args: e)));
            }

            _omnisharpProcess = new AsyncLazy<Process>(StartOmniSharp);

            async Task<Process> StartOmniSharp()
            {
                using (var operation = Log.OnEnterAndConfirmOnExit())
                {
                    var omnisharpExe = await MLS.Agent.Tools.OmniSharp.EnsureInstalledOrAcquire();

                    var process =
                        CommandLine.StartProcess(
                            omnisharpExe.FullName,
                            string.IsNullOrWhiteSpace(pluginPath)
                                ? ""
                                : $"-pl {pluginPath}",
                            projectDirectory,
                            _standardOutput.OnNext,
                            _standardError.OnNext);

                    _disposables.Add(() =>
                    {
                        if (!process.HasExited)
                        {
                            process.Kill();
                        }
                    });

                    _disposables.Add(process);

                    await StandardOutput
                          .AsOmniSharpMessages()
                          .OfType<OmniSharpEventMessage<ProjectAdded>>()
                          .FirstAsync();

                    operation.Succeed();

                    return process;
                }
            }
        }

        public IObservable<string> StandardOutput => _standardOutput;

        public IObservable<string> StandardError => _standardError;

        public async Task Send(string text)
        {
            var process = await _omnisharpProcess.ValueAsync();
            process.StandardInput.WriteLine(text);
        }

        public void Dispose() => _disposables.Dispose();

        public int NextSeq() => Interlocked.Increment(ref _seq);

        public async Task WorkspaceReady(TimeBudget budget = null) =>
            await _omnisharpProcess.ValueAsync()
                                   .CancelIfExceeds(budget ?? TimeBudget.Unlimited());
    }
}
