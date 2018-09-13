using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Clockwise;
using Newtonsoft.Json;
using Pocket;
using WorkspaceServer;
using WorkspaceServer.Models;
using static Pocket.Logger<MLS.Agent.Warmup>;
using Workspace = WorkspaceServer.Models.Execution.Workspace;

namespace MLS.Agent
{
    public class Warmup : HostedService
    {
        private readonly WorkspaceRegistry workspaceRegistry;

        private readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:4242")
        };

        public Warmup(WorkspaceRegistry workspaceRegistry)
        {
            this.workspaceRegistry = workspaceRegistry ??
                                     throw new ArgumentNullException(nameof(workspaceRegistry));
        }

        protected override async Task ExecuteAsync(Budget budget)
        {
            await Task.WhenAll(
                workspaceRegistry.Select(async builder =>
                {
                    var build = await builder.GetWorkspaceBuild(budget);
                    await build.EnsureReady(budget);
                }));

            await WarmUpRoutes();
        }

        private async Task WarmUpRoutes()
        {
            using (var operation = Log.OnEnterAndExit())
            {
                await _httpClient.GetAsync("/sensors/version");

                await WarmpUpRoute("/workspace/run");
                await WarmpUpRoute("/workspace/completion");
                await WarmpUpRoute("/workspace/diagnostics");
                await WarmpUpRoute("/workspace/signaturehelp");

                async Task WarmpUpRoute(string relativeUri)
                {
                    const string code = "Console.WriteLine(42);";

                    var workspaceRequest = new WorkspaceRequest(
                        activeBufferId: "Program.cs",
                        workspace: new Workspace(
                            workspaceType: "console",
                            buffers: new[]
                            {
                                new Workspace.Buffer("Program.cs", code, 0)
                            }));

                    var response = await Post(relativeUri,
                                              workspaceRequest);

                    operation.Info("WarmUp response from {relativeUri} {response}", relativeUri, response);
                }
            }
        }

        private async Task<HttpResponseMessage> Post(string relativeUri, WorkspaceRequest workspaceRequest) =>
            await _httpClient.PostAsync(
                relativeUri,
                new StringContent(
                    JsonConvert.SerializeObject(workspaceRequest), Encoding.UTF8, "application/json"));
    }
}
