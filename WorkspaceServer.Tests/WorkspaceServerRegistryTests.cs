using System;
using System.Diagnostics;
using System.Threading.Tasks;
using FluentAssertions;
using WorkspaceServer.Models.Execution;
using Xunit;

namespace WorkspaceServer.Tests
{
    public class WorkspaceServerRegistryTests
    {
        [Fact]
        public async Task Workspaces_can_be_registered_to_be_created_using_dotnet_new()
        {
            using (var registry = new WorkspaceServerRegistry())
            {
                var workspaceId = Create.TestFolder().Name;

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
            using (var registry = new WorkspaceServerRegistry())
            {
                var workspaceId = Create.TestFolder().Name;

                registry.AddWorkspace(workspaceId,
                                      options =>
                                      {
                                          options.CreateUsingDotnet("console");
                                          options.AddPackageReference("Twilio", "5.9.2");
                                      });

                var workspaceServer = await registry.GetWorkspaceServer(workspaceId);

                var result = await workspaceServer.Run(new RunRequest(@"
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
}"));

                result.Succeeded.Should().BeTrue(because: "compilation can't succeed unless the NuGet package has been restored.");
            }
        }

        [Fact]
        public async Task Workspace_servers_that_have_been_started_are_disposed_when_registry_is_disposed()
        {
            IWorkspaceServer workspaceServer;

            using (var registry = new WorkspaceServerRegistry())
            {
                var workspaceId = Default.TemplateWorkspace.Name;

                registry.AddWorkspace(workspaceId,
                                      options => options.CreateUsingDotnet("console"));

                workspaceServer = await registry.GetWorkspaceServer(workspaceId);
            }

            Func<Task> dispose = async () => await workspaceServer.Run(Create.SimpleRunRequest());

            dispose.ShouldThrow<ObjectDisposedException>();
        }

        [Fact]
        public async Task All_workspace_servers_can_be_started_proactively()
        {
            using (var registry = new WorkspaceServerRegistry())
            {
                var name = nameof(All_workspace_servers_can_be_started_proactively);
                registry.AddWorkspace($"{name}.1",
                                      options => options.CreateUsingDotnet("console"));
                registry.AddWorkspace($"{name}.2",
                                      options => options.CreateUsingDotnet("console"));

                await registry.StartAllServers();

                var stopwatch = Stopwatch.StartNew();

                await registry.GetWorkspaceServer($"{name}.1");

                stopwatch.ElapsedMilliseconds.Should().BeLessThan(5);
            }
        }
    }
}
