using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Clockwise;
using MLS.Agent.Tools;
using OmniSharp.Client;
using OmniSharp.Client.Events;
using Pocket;
using WorkspaceServer.WorkspaceFeatures;
using static Pocket.Logger<WorkspaceServer.Servers.Dotnet.OmniSharpServer>;

namespace WorkspaceServer.Servers.Dotnet
{
    public class OmniSharpServer : IDisposable
    {
        private readonly DirectoryInfo _projectDirectory;
        private readonly string _pluginPath;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private int _seq;
        private readonly StandardOutput _standardOutput = new StandardOutput();
        private readonly StandardError _standardError = new StandardError();
        private readonly AsyncLazy<Process> _omnisharpProcess;

        public OmniSharpServer(
            DirectoryInfo projectDirectory,
            string pluginPath = null,
            bool logToPocketLogger = false)
        {
            _projectDirectory = projectDirectory ??
                                throw new ArgumentNullException(nameof(projectDirectory));

            _pluginPath = pluginPath;

            if (logToPocketLogger)
            {
                _disposables.Add(StandardOutput.Subscribe(e => Log.Info("{message}", args: e)));
                _disposables.Add(StandardError.Subscribe(e => Log.Error("{message}", args: e)));
            }

            var dotTdn = new FileInfo(Path.Combine(projectDirectory.FullName, ".trydotnet"));
            _omnisharpProcess = new AsyncLazy<Process>(() => StartOmniSharp(dotTdn));
        }

        private async Task<Process> StartOmniSharp(FileInfo dotTryDotNetPath)
        {
            using (var operation = Log.OnEnterAndConfirmOnExit())
            {
                var omnisharpExe = await MLS.Agent.Tools.OmniSharp.EnsureInstalledOrAcquire(dotTryDotNetPath);

                var process =
                    CommandLine.StartProcess(
                        omnisharpExe.FullName,
                        string.IsNullOrWhiteSpace(_pluginPath)
                            ? ""
                            : $"-pl {_pluginPath}",
                        _projectDirectory,
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

        public StandardOutput StandardOutput => _standardOutput;

        public StandardError StandardError => _standardError;

        public async Task Send(string text)
        {
            var process = await _omnisharpProcess.ValueAsync();
            process.StandardInput.WriteLine(text);
        }

        public void Dispose() => _disposables.Dispose();

        public int NextSeq() => Interlocked.Increment(ref _seq);

        public async Task WorkspaceReady(Budget budget = null) =>
            await _omnisharpProcess.ValueAsync()
                                   .CancelIfExceeds(budget ?? new Budget());
    }
}
