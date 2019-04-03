using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using MLS.Agent.Tools;
using WorkspaceServer.Packaging;
using Xunit;
using System.Linq;
using MLS.Agent.CommandLine;
using System.CommandLine;
using MLS.Agent;

namespace WorkspaceServer.Tests
{
    public class PrebuiltBlazorPackageLocatorTests
    {
        AsyncLazy<(string, DirectoryInfo)> _tool = new AsyncLazy<(string, DirectoryInfo)>(async () =>
        {
            var asset = await Create.NetstandardWorkspaceCopy();
            var name = Path.GetFileNameWithoutExtension(asset.Directory.GetFiles("*.csproj").First().Name);
            string packageName = $"dotnettry.{name}";
            var console = new TestConsole();
            await PackCommand.Do(new PackOptions(asset.Directory, enableBlazor: true), console);
            var nupkg = asset.Directory
                .GetFiles("*.nupkg").Single();

            return (packageName, nupkg.Directory);
        });

        [Fact]
        public async Task Discovers_built_blazor_package()
        {
            var (packageName, addSource) = await _tool.ValueAsync();

            using (var directory = DisposableDirectory.Create())
            {
                await InstallCommand.Do(new InstallOptions(addSource, packageName, directory.Directory), new TestConsole());

                var exe = Path.Combine(directory.Directory.FullName, packageName);
                var result = await MLS.Agent.Tools.CommandLine.Execute(exe, "locate-projects", workingDir: directory.Directory);
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
            var (packageName, addSource) = await _tool.ValueAsync();

            using (var directory = DisposableDirectory.Create())
            {
                var dotnet = new Dotnet(directory.Directory);
                var result = await dotnet.ToolInstall(packageName, directory.Directory, addSource);

                var locator = new PrebuiltBlazorPackageLocator(directory.Directory);
                var things = await locator.Discover();
                things.Should().BeEmpty();
            }
        }
    }
}