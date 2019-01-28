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

            Directory.CreateDirectory(wwwRootPath);
            var guid = Guid.NewGuid().ToString("N");
            File.WriteAllText(Path.Combine(wwwRootPath, "dataFile.txt"), guid);

            using (var agent = new AgentService(new StartupOptions(rootDirectory: TestAssets.SampleConsole)))
            {
                var response = await agent.GetAsync(@"/dataFile.txt");

                response.Should().BeSuccessful();

                var content = await response.Content.ReadAsStringAsync();

                content.Should().Be(guid);
            }
        }

        [Fact]
        public async Task Static_files_can_be_loaded_from_wwwroot_in_root_directory_first()
        {
            var wwwRootPath = Path.Combine(Path.GetDirectoryName(typeof(Startup).Assembly.Location), "wwwroot");
            var localWwwRootPath = Path.Combine(TestAssets.SampleConsole.FullName, "wwwroot");

            Directory.CreateDirectory(wwwRootPath);
            Directory.CreateDirectory(localWwwRootPath);

            var rootContent = Guid.NewGuid().ToString("N");
            File.WriteAllText(Path.Combine(wwwRootPath, "dataFile.txt"), rootContent);

            var localContent = Guid.NewGuid().ToString("N");
            File.WriteAllText(Path.Combine(localWwwRootPath, "dataFile.txt"), localContent);

            using (var agent = new AgentService(new StartupOptions(rootDirectory: TestAssets.SampleConsole)))
            {
                var response = await agent.GetAsync(@"/dataFile.txt");

                response.Should().BeSuccessful();

                var content = await response.Content.ReadAsStringAsync();

                content.Should().Be(localContent);
            }
        }
    }
}