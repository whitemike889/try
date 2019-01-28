using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Recipes;
using Xunit;

namespace MLS.Agent.Tests
{
    public class StaticFileTests
    {
        [Fact]
        public async Task Static_files_can_be_loaded_from_wwwroot_at_agent_folder()
        {
            var wwwRootPath = Path.Combine(Path.GetDirectoryName(typeof(Startup).Assembly.Location), "wwwroot");
            var fileName = Guid.NewGuid().ToString("N") + ".txt";
            Directory.CreateDirectory(wwwRootPath);
            var guid = Guid.NewGuid().ToString("N");
            File.WriteAllText(Path.Combine(wwwRootPath, fileName), guid);

            using (var agent = new AgentService(new StartupOptions(rootDirectory: TestAssets.SampleConsole)))
            {
                var response = await agent.GetAsync($@"/{fileName}");

                response.Should().BeSuccessful();

                var content = await response.Content.ReadAsStringAsync();

                content.Should().Be(guid);
            }
        }
    }
}