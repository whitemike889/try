using FluentAssertions;
using MLS.Agent.Tools;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Text;
using System.Threading.Tasks;
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
                await PackageCommand.Do(asset, temp, console);
                var result = await CommandLine.Execute("dotnet", $"tool install --add-source {temp.FullName} BasicConsoleApp --tool-path {temp.FullName}");
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
    }
}
