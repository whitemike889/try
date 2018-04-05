using System;
using System.Net.Http;
using System.Threading.Tasks;
using Clockwise;
using Newtonsoft.Json;
using Pocket;
using WorkspaceServer;
using WorkspaceServer.Models.Execution;
using static Pocket.Logger<MLS.Agent.Warmup>;

namespace MLS.Agent
{
    public class Warmup : HostedService
    {
        private readonly WorkspaceServerRegistry workspaceServerRegistry;

        private readonly HttpClient httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://127.0.0.1:4242")
        };

        public Warmup(WorkspaceServerRegistry workspaceServerRegistry)
        {
            this.workspaceServerRegistry = workspaceServerRegistry ??
                                           throw new ArgumentNullException(nameof(workspaceServerRegistry));
        }

        protected override async Task ExecuteAsync(Budget budget)
        {
            await WarmUpRoutes();

            await workspaceServerRegistry.StartAllServers(budget);
        }

        private async Task WarmUpRoutes()
        {
            using (Log.OnEnterAndExit())
            {
                await httpClient.GetAsync("/sensors/version");

                await Post("/workspace/run",
                           new WorkspaceRequest(
                               new Workspace(
                                   buffers: new[]
                                   {
                                       new Workspace.Buffer("Program.cs", "Console.WriteLine(42);", 0)
                                   })));
            }
        }

        private async Task Post(string relativeUri, WorkspaceRequest workspaceRequest) =>
            await httpClient.PostAsync(
                relativeUri,
                new StringContent(
                    JsonConvert.SerializeObject(
                        workspaceRequest)));
    }
}
