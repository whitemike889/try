using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using MLS.Agent.CommandLine;
using Pocket;
using Recipes;
using WorkspaceServer;

namespace MLS.Agent.Tests
{
    public class AgentService : IDisposable
    {
        private readonly IDirectoryAccessor _directoryAccessor;
        private readonly StartupOptions _options;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        private readonly HttpClient _client;

        public AgentService(StartupOptions options = null, IDirectoryAccessor directoryAccessor = null)
        {
            _directoryAccessor = directoryAccessor;
            _options = options ?? new StartupOptions(
                           production: false,
                           languageService: false);

            var testServer = CreateTestServer();

            _client = testServer.CreateClient();

            _disposables.Add(testServer);
            _disposables.Add(_client);
        }

        public FakeBrowserLauncher BrowserLauncher { get; private set; }

        public void Dispose() => _disposables.Dispose();

        private TestServer CreateTestServer() => new TestServer(CreateWebHostBuilder());

        private IWebHostBuilder CreateWebHostBuilder()
        {
            var builder = new WebHostBuilder()
                          .ConfigureServices(c =>
                          {
                              c.AddSingleton(_options);
                              c.AddSingleton<IBrowserLauncher>(sp =>
                              {
                                  BrowserLauncher = new FakeBrowserLauncher(_directoryAccessor??sp.GetRequiredService<IDirectoryAccessor>());
                                  return BrowserLauncher;
                              });
                          })
                          .UseTestEnvironment()
                          .UseStartup<Startup>();

            return builder;
        }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request) =>
            _client.SendAsync(request);
    }
}