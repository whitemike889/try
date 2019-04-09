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

            var exe = Path.Combine(addSource.FullName, packageName);
            var result = await CommandLine.Execute(exe, "locate-projects");
            foreach (var subdir in new DirectoryInfo(result.Output.First()).GetDirectories())
            {
                await new Dotnet(subdir).Build("-o runtime / bl");
            }

            var locator = new PrebuiltBlazorPackageLocator();
            var package = await locator.Locate(packageName);
            package.Name.Should().Be(packageName);
        }

        [Fact]
        public async Task Does_not_discover_unbuilt_blazor_package()
        {
            var (packageName, addSource) = await Create.NupkgWithBlazorEnabled();

            var dotnet = new Dotnet(addSource);
            await dotnet.ToolInstall(packageName, Package.DefaultPackagesDirectory, addSource);

            var locator = new PrebuiltBlazorPackageLocator();
            var package = await locator.Locate(packageName);
            package.Should().BeNull();
        }
    }
}