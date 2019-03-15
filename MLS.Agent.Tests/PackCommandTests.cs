using FluentAssertions;
using System.CommandLine;
using System.Threading.Tasks;
using MLS.Agent.CommandLine;
using Xunit;
using WorkspaceServer.Tests;

namespace MLS.Agent.Tests
{
    public class PackCommandTests
    {
        [Fact]
        public async Task Pack_project_works()
        {
            var asset = await Create.ConsoleWorkspaceCopy();

            var console = new TestConsole();

            await PackCommand.Do(new PackOptions(asset.Directory), console);

            asset.Directory
                 .GetFiles()
                 .Should()
                 .Contain(f => f.Name.Contains("nupkg"));
        }
        [Fact]
        public async Task Pack_project_works_with_blazor()
        {
            var asset = await Create.NetstandardWorkspaceCopy();

            var console = new TestConsole();

            await PackCommand.Do(new PackOptions(asset.Directory, enableBlazor: true), console);

            asset.Directory
                 .GetFiles()
                 .Should()
                 .Contain(f => f.Name.Contains("nupkg"));
        }

    }
}
