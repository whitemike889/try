using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Clockwise;
using Newtonsoft.Json;
using Pocket;
using WorkspaceServer;
using WorkspaceServer.Models;
using WorkspaceServer.Models.Execution;
using static Pocket.Logger<MLS.Agent.Warmup>;

namespace MLS.Agent
{
    public class Warmup : HostedService
    {
        private readonly WorkspaceServerRegistry _workspaceServerRegistry;

        private readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://127.0.0.1:4242")
        };

        public Warmup(WorkspaceServerRegistry workspaceServerRegistry)
        {
            this._workspaceServerRegistry = workspaceServerRegistry ??
                                           throw new ArgumentNullException(nameof(workspaceServerRegistry));
        }

        protected override async Task ExecuteAsync(Budget budget)
        {
            await WarmUpRoutes();

            await _workspaceServerRegistry.StartAllServers(budget);
        }

        private async Task WarmUpRoutes()
        {
            using (Log.OnEnterAndExit())
            {
                await _httpClient.GetAsync("/sensors/version");

                var response = await Post("/workspace/run",
                           new WorkspaceRequest(
                               new Workspace(
                                   buffers: new[]
                                   {
                                       new Workspace.Buffer("Program.cs", "Console.WriteLine(42);", 0)
                                   })));

                Log.Info("WarmUp response {response}", response);
            }
        }

        private async Task<HttpResponseMessage> Post(string relativeUri, WorkspaceRequest workspaceRequest) =>
            await _httpClient.PostAsync(
                relativeUri,
                new StringContent(
                    JsonConvert.SerializeObject(workspaceRequest), Encoding.UTF8, "application/json"));
    }
}
