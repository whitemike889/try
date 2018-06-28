using System;
using System.Threading.Tasks;
using Clockwise;
using FluentAssertions;
using Pocket;
using WorkspaceServer.Models;
using WorkspaceServer.Servers.Roslyn;
using Xunit;
using Xunit.Abstractions;
using Workspace = MLS.Agent.Tools.Workspace;

namespace WorkspaceServer.Tests
{
    public class WorkspaceServerRegistryTests : IDisposable
    {
        private readonly CompositeDisposable disposables = new CompositeDisposable();

        public WorkspaceServerRegistryTests(ITestOutputHelper output)
        {
            disposables.Add(output.SubscribeToPocketLogger());
            disposables.Add(VirtualClock.Start());
        }

        public void Dispose() => disposables.Dispose();

        [Fact]
        public async Task Workspaces_can_be_registered_to_be_created_using_dotnet_new()
        {
            using (var registry = new WorkspaceRegistry())
            {
                var workspaceId = Workspace.CreateDirectory(nameof(Workspaces_can_be_registered_to_be_created_using_dotnet_new)).Name;

                registry.AddWorkspace(workspaceId,
                                      options => options.CreateUsingDotnet("console"));

                var workspace = await registry.GetWorkspace(workspaceId);

                await workspace.EnsureCreated();

                workspace.Directory.GetFiles().Length.Should().BeGreaterThan(1);
            }
        }

        [Fact]
        public async Task NuGet_packages_can_be_added_during_initialization()
        {
            using (var registry = new WorkspaceRegistry())
            {
                var workspaceId = Workspace.CreateDirectory(nameof(NuGet_packages_can_be_added_during_initialization)).Name;

                registry.AddWorkspace(workspaceId,
                                      options =>
                                      {
                                          options.CreateUsingDotnet("console");
                                          options.AddPackageReference("Twilio", "5.9.2");
                                      });

                var workspaceServer = new RoslynWorkspaceServer(registry);

                var workspace = new Models.Execution.Workspace(
                    @"
using System;
using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Twilio_try.dot.net_sample
{
    class Program
    {
        static void Main()
        {
            var sendFromPhoneNumber = new PhoneNumber(""TWILIO_PHONE_NUMBER"");
            var sendToPhoneNumber = new PhoneNumber(""RECIPIENT_PHONE_NUMBER"");
        }
    }
}",
                    workspaceType: workspaceId);

                var result = await workspaceServer.Run(workspace);

                result.Succeeded.Should().BeTrue(because: "compilation can't succeed unless the NuGet package has been restored.");
            }
        }

        [Fact]
        public async Task GetWorkspace_will_check_workspaces_directory_if_requested_workspace_was_not_registered()
        {
            var unregisteredWorkspace = await Default.ConsoleWorkspace;

            using (var registry = new WorkspaceRegistry())
            {
                var resolvedWorkspace = await registry.GetWorkspace(unregisteredWorkspace.Name);

                resolvedWorkspace.Directory.FullName.Should().Be(unregisteredWorkspace.Directory.FullName);
                resolvedWorkspace.IsCreated.Should().BeTrue();
                resolvedWorkspace.IsBuilt.Should().BeTrue();
            }
        }

        [Fact]
        public async Task When_workspace_was_not_registered_then_GetWorkspaceServer_will_return_a_working_server()
        {
            var unregisteredWorkspace = await Default.ConsoleWorkspace;

            using (var registry = new WorkspaceRegistry())
            {
                var server = new RoslynWorkspaceServer(registry);

                var workspaceRequest = WorkspaceRequest.FromDirectory(unregisteredWorkspace.Directory, unregisteredWorkspace.Name);

                var result = await server.Run(workspaceRequest.Workspace);

                result.Succeeded.Should().BeTrue();
            }
        }
    }
}
