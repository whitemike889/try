using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using MLS.Agent.Tools;
using Xunit;
using System.Linq;
using MLS.Agent.CommandLine;
using System.CommandLine;
using MLS.Agent;

namespace WorkspaceServer.Tests
{
    public class PrebuiltBlazorPackageLocatorTests
    {
        [Fact]
        public async Task Discovers_built_blazor_package()
        {
            var (packageName, addSource) = await Create.NupkgWithBlazorEnabled();

            using (var directory = DisposableDirectory.Create())
            {
                await InstallCommand.Do(new InstallOptions(addSource, packageName, directory.Directory), new TestConsole());

                var exe = Path.Combine(directory.Directory.FullName, packageName);
                var result = await CommandLine.Execute(exe, "locate-projects", workingDir: directory.Directory);
                foreach (var subdir in new DirectoryInfo(result.Output.First()).GetDirectories())
                {
                    await (new Dotnet(subdir).Build("-o runtime / bl"));
                }

                var locator = new PrebuiltBlazorPackageLocator(directory.Directory);
                var things = await locator.Discover();
                things.Should().NotBeEmpty();
            }
        }

        [Fact]
        public async Task Does_not_discover_unbuilt_blazor_package()
        {
            var (packageName, addSource) = await Create.NupkgWithBlazorEnabled();

            using (var directory = DisposableDirectory.Create())
            {
                var dotnet = new Dotnet(directory.Directory);
                await dotnet.ToolInstall(packageName, directory.Directory, addSource);

                var locator = new PrebuiltBlazorPackageLocator(directory.Directory);
                var things = await locator.Discover();
                things.Should().BeEmpty();
            }
        }

      
    }
}