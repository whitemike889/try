using System;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Clockwise;
using FluentAssertions;
using FluentAssertions.Extensions;
using Pocket;
using Recipes;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Servers.InMemory;
using WorkspaceServer.WorkspaceFeatures;
using Xunit;
using Xunit.Abstractions;
using Workspace = MLS.Agent.Tools.Workspace;

namespace WorkspaceServer.Tests
{
    public class AspNetWorkspaceTests : IDisposable
    {
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public AspNetWorkspaceTests(ITestOutputHelper output)
        {
            _disposables.Add(output.SubscribeToPocketLogger());
        }

        public void Dispose() => _disposables.Dispose();

        [Fact(Skip = "we broke this")]
        public async Task Run_starts_the_kestrel_server_and_provides_a_WebServer_feature_that_can_receive_requests()
        {
            var (server, workspace) = await GetWorkspaceAndServer();

            using (var runResult = await server.Run(Models.Execution.Workspace.FromDirectory(workspace.Directory, workspace.Name)))
            {
                var webServer = runResult.GetFeature<WebServer>();

                var response = await webServer.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/api/values")).CancelIfExceeds(new TimeBudget(35.Seconds()));

                var result = await response.EnsureSuccess()
                                           .DeserializeAs<string[]>();

                result.Should().Equal("value1", "value2");
            }
        }

        protected async Task<(RoslynWorkspaceServer server, Workspace workspace )> GetWorkspaceAndServer(
            [CallerMemberName] string testName = null)
        {
            var registry = new WorkspaceRegistry();

            registry.AddWorkspace(testName, builder => { builder.CreateCopyOf("aspnet.webapi"); });

            var workspace = await registry.GetWorkspace(testName);

            var server = new RoslynWorkspaceServer(registry);

            return (server, workspace);
        }
    }
}
