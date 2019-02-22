using FluentAssertions;
using System.CommandLine;
using System.Threading.Tasks;
using MLS.Agent.CommandLine;
using Xunit;

namespace MLS.Agent.Tests
{
    public class PackCommandTests
    {
        [Fact]
        public async Task Pack_project_works()
        {
            var asset = TestAssets.SampleConsole;

            foreach (var file in asset.GetFiles("*.nupkg"))
            {
                file.Delete();
            }

            var console = new TestConsole();
            await PackCommand.Do(new PackOptions(asset), console);
            asset.GetFiles()
                .Should().Contain(f => f.Name.Contains("nupkg"));
        }
    }
}
