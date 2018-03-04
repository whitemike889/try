using FluentAssertions;
using MLS.Agent.JsonContracts;
using Newtonsoft.Json;
using WorkspaceServer.Models.Execution;
using Xunit;

namespace WorkspaceServer.Tests
{
    public class JsonContractsTests
    {
        [Fact]
        public void can_augment_workspace_to_workpaceRequest()
        {
            JsonContratcs.Setup();

            var json = JsonConvert.SerializeObject(new
            {
                workspaceType = "console",
                buffers = new[] { new { id = "testId", content = "no code", position = 0 } }
            });

            var request = JsonConvert.DeserializeObject<WorkspaceRequest>(json);
            request.Should().NotBeNull();
        }
    }
}
