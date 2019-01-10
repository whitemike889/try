using System;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Clockwise;
using FluentAssertions;
using FluentAssertions.Extensions;
using MLS.Protocol;
using MLS.Protocol.Execution;
using Pocket;
using Recipes;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Servers.Roslyn;
using WorkspaceServer.Features;
using WorkspaceServer.Packaging;
using Xunit;
using Xunit.Abstractions;

namespace WorkspaceServer.Tests
{
    public class AspNetWorkspaceTests : IDisposable
    {
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public AspNetWorkspaceTests(ITestOutputHelper output)
        {
            _disposables.Add(output.SubscribeToPocketLogger());
            _disposables.Add(VirtualClock.Start());
        }

        public void Dispose() => _disposables.Dispose();

        [Fact]
        public async Task Run_starts_the_kestrel_server_and_provides_a_WebServer_feature_that_can_receive_requests()
        {
            var (server, build) = await GetRunnerAndWorkspace();

            var workspace = WorkspaceFactory.CreateWorkspaceFromDirectory(build.Directory, build.Name);

            using (var runResult = await server.Run(new WorkspaceRequest(workspace, "Program.cs")))
            {
                var webServer = runResult.GetFeature<WebServer>();

                var response = await webServer.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/api/values")).CancelIfExceeds(new TimeBudget(35.Seconds()));

                var result = await response.EnsureSuccess()
                                           .DeserializeAs<string[]>();

                result.Should().Equal("value1", "value2");
            }
        }

        protected async Task<(ICodeRunner server, Package workspace)> GetRunnerAndWorkspace(
            [CallerMemberName] string testName = null)
        {
            var workspace = await Create.WebApiWorkspaceCopy(testName);

            var server = new RoslynWorkspaceServer(workspace);

            return (server, workspace);
        }
    }
}
