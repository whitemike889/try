using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Pocket;
using Recipes;
using WorkspaceServer;

namespace MLS.Agent.Tests
{
    public class AgentService : IDisposable
    {
        private readonly WorkspaceServerRegistry workspaceServerRegistry;
        private readonly CompositeDisposable disposables = new CompositeDisposable();

        private readonly HttpClient client;

        public AgentService(WorkspaceServerRegistry workspaceServerRegistry = null)
        {
            this.workspaceServerRegistry = workspaceServerRegistry;
            var testServer = CreateTestServer();

            client = testServer.CreateClient();

            disposables.Add(testServer);
            disposables.Add(client);
        }

        public void Dispose() => disposables.Dispose();

        private TestServer CreateTestServer() => new TestServer(CreateWebHostBuilder());

        private IWebHostBuilder CreateWebHostBuilder()
        {
            var builder = new WebHostBuilder()
                          .UseTestEnvironment()
                          .UseStartup<Startup>();

            builder.ConfigureServices(services =>
            {
                if (workspaceServerRegistry != null)
                {
                    services.AddSingleton(workspaceServerRegistry);
                }
            });

            return builder;
        }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request) =>
            client.SendAsync(request);
    }
}
