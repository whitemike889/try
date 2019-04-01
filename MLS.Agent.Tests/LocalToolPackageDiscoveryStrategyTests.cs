using FluentAssertions;
using System.CommandLine;
using System.Threading.Tasks;
using MLS.Agent.CommandLine;
using MLS.Agent.Tests.TestUtility;
using WorkspaceServer;
using WorkspaceServer.PackageDiscovery;
using Xunit;

namespace MLS.Agent.Tests
{
    public class LocalToolPackageDiscoveryStrategyTests
    {
        [Fact]
        public async Task Discover_tool_from_directory()
        {
            using (var directory = DisposableDirectory.Create())
            {
                var console = new TestConsole();
                var temp = directory.Directory;
                var asset = TestAssets.SampleConsole;
                await PackCommand.Do(new PackOptions(asset, temp), console);
                var result = await Tools.CommandLine.Execute("dotnet", $"tool install --add-source {temp.FullName} BasicConsoleApp --tool-path {temp.FullName}");
                result.ExitCode.Should().Be(0);

                var strategy = new LocalToolPackageDiscoveryStrategy(temp);
                var tool = await strategy.Locate(new PackageDescriptor("BasicConsoleApp"));
                tool.Should().NotBeNull();
                tool.PackageInitializer.Should().BeOfType<PackageToolInitializer>();
            }
        }

        [Fact]
        public void Does_not_throw_for_missing_tool()
        {
            using (var directory = DisposableDirectory.Create())
            {
                var temp = directory.Directory;
                var strategy = new LocalToolPackageDiscoveryStrategy(temp);

                strategy.Invoking(s => s.Locate(new PackageDescriptor("not-a-workspace")).Wait()).Should().NotThrow();
            }
        }

        [Fact]
        public async Task Installs_tool_from_package_source_when_requested()
        {
            var console = new TestConsole();
            var asset = await LocalToolHelpers.CreateTool(console);

            var strategy = new LocalToolPackageDiscoveryStrategy(asset, asset);
            var package = await strategy.Locate(new PackageDescriptor("console"));
            package.Should().NotBeNull();
        }
    }
}
