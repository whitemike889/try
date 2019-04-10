using System;
using System.CommandLine;
using System.IO;
using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;
using MLS.Agent.CommandLine;
using MLS.Agent.Tools;
using WorkspaceServer.Packaging;
using Xunit;

namespace WorkspaceServer.Tests
{
    public class PrebuiltBlazorPackageLocatorTests
    {
        [Fact]
        public async Task Discovers_built_blazor_package()
        {
            var (packageName, addSource) = await Create.NupkgWithBlazorEnabled();

            await InstallCommand.Do(new InstallOptions(addSource, packageName), new TestConsole());
            var locator = new PrebuiltBlazorPackageLocator();
            var package = await locator.Locate(packageName);
            package.Name.Should().Be($"runner-{packageName}");
        }
    }
}