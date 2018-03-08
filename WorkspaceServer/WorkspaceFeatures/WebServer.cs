using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using MLS.Agent.Tools;
using Pocket;
using Recipes;
using static Pocket.Logger;

namespace WorkspaceServer.WorkspaceFeatures
{
    public class WebServer : IDisposable
    {
        private readonly AsyncLazy<HttpClient> _getHttpClient;
        private readonly AsyncLazy<Uri> _started;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private static int _port = 6000;

        public WebServer(Workspace workspace)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            Interlocked.Increment(ref _port);

            _started = new AsyncLazy<Uri>(async () =>
            {
                await workspace.EnsurePublished();

                var environmentVariables = new List<(string, string)>();

                foreach (DictionaryEntry e in Environment.GetEnvironmentVariables())
                {
                    environmentVariables.Add((e.Key.ToString(), e.Value.ToString()));
                }

                Log.Trace("Starting Kestrel on port {port}. Environment: {env}",
                          _port, 
                          environmentVariables);

                var process = CommandLine.StartProcess(
                    DotnetMuxer.Path.FullName,
                    workspace.EntryPointAssemblyPath.FullName,
                    workspace.Directory,
                    StandardOutput.OnNext,
                    StandardError.OnNext,
                    ("ASPNETCORE_URLS", $"http://127.0.0.1:0"),
                    ("ASPNETCORE_PORT", null));

                _disposables.Add(() => {
                    process.Kill();
                });

                _disposables.Add(StandardOutput.Subscribe(s => Log.Trace(s)));
                _disposables.Add(StandardError.Subscribe(s => Log.Error(s)));

                var kestrelListeningMessagePrefix = "Now listening on: ";

                var uriString = await StandardOutput
                                      .Where(line => line.StartsWith(kestrelListeningMessagePrefix))
                                      .Select(line => line.Replace(kestrelListeningMessagePrefix, ""))
                                      .FirstAsync();

                return new Uri(uriString);
            });

            _getHttpClient = new AsyncLazy<HttpClient>(async () =>
            {
                var httpClient = new HttpClient
                {
                    BaseAddress = await EnsureStarted()
                };

                return httpClient;
            });
        }

        public StandardOutput StandardOutput { get; } = new StandardOutput();

        public StandardError StandardError { get; } = new StandardError();

        public Task<Uri> EnsureStarted() => _started.ValueAsync();

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            var httpClient = await _getHttpClient.ValueAsync();

            return await httpClient.SendAsync(request);
        }

        public void Dispose() => _disposables.Dispose();
    }
}
