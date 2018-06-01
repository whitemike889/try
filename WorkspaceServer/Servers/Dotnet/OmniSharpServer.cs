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

namespace WorkspaceServer.Servers.Dotnet
{
    public class OmniSharpServer : IDisposable
    {
        private readonly DirectoryInfo _projectDirectory;
        private readonly string _pluginPath;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private int _seq;
        private readonly AsyncLazy<Process> _omnisharpProcess;
        private readonly Logger _log;

        public OmniSharpServer(
            DirectoryInfo projectDirectory,
            string pluginPath = null,
            bool logToPocketLogger = false)
        {
            _projectDirectory = projectDirectory ??
                                throw new ArgumentNullException(nameof(projectDirectory));

            _pluginPath = pluginPath;

            if (!string.IsNullOrWhiteSpace(_pluginPath))
            {
                var fileInfo = new FileInfo(_pluginPath);
                if (!fileInfo.Exists)
                {
                    throw new FileNotFoundException($"Cannot locate plugin {_pluginPath}");
                }
            }

            _log = new Logger($"{nameof(OmniSharpServer)}:{projectDirectory.Name}");

            if (logToPocketLogger)
            {
                _disposables.Add(StandardOutput.Subscribe(e => _log.Info("{message}", args: e)));
                _disposables.Add(StandardError.Subscribe(e => _log.Error("{message}", args: e)));
            }

            var dotTdn = new FileInfo(Path.Combine(projectDirectory.FullName, ".trydotnet"));
            
            _omnisharpProcess = new AsyncLazy<Process>(() => StartOmniSharp(dotTdn));
        }

        private async Task<Process> StartOmniSharp(FileInfo dotTryDotNetPath)
        {
            using (var operation = _log.OnEnterAndConfirmOnExit())
            {
                var omnisharpExe = await MLS.Agent.Tools.OmniSharp.EnsureInstalledOrAcquire(dotTryDotNetPath);

                var process =
                    CommandLine.StartProcess(
                        omnisharpExe.FullName,
                        string.IsNullOrWhiteSpace(_pluginPath)
                            ? ""
                            : $"-pl {_pluginPath}",
                        _projectDirectory,
                        StandardOutput.OnNext,
                        StandardError.OnNext);

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

        public StandardOutput StandardOutput { get; } = new StandardOutput();

        public StandardError StandardError { get; } = new StandardError();

        public async Task Send(string text)
        {
            var process = await _omnisharpProcess.ValueAsync();
            process.StandardInput.WriteLine(text);
        }

        public void Dispose() => _disposables.Dispose();

        public int NextSeq() => Interlocked.Increment(ref _seq);

        public async Task WorkspaceReady(Budget budget = null)
        {
            await _omnisharpProcess.ValueAsync()
                .CancelIfExceeds(budget ?? new Budget());
            budget?.RecordEntry();
        }
    }
}
