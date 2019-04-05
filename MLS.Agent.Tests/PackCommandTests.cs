using FluentAssertions;
using System.CommandLine;
using System.Threading.Tasks;
using MLS.Agent.CommandLine;
using Xunit;
using WorkspaceServer.Tests;
using WorkspaceServer;
using System.Linq;
using System.IO;

namespace MLS.Agent.Tests
{
    public class PackCommandTests
    {
        [Fact]
        public async Task Pack_project_creates_a_nupkg_with_passed_version()
        {
            var asset = await Create.NetstandardWorkspaceCopy();

            var console = new TestConsole();

            await PackCommand.Do(new PackOptions(asset.Directory, "3.4.5"), console);

            asset.Directory
                 .GetFiles()
                 .Should()
                 .Contain(f => f.Name.Contains("3.4.5.nupkg"));
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

        [Fact]
        public async Task Pack_project_blazor_contents()
        {
            var asset = await Create.NetstandardWorkspaceCopy();

            var name = Path.GetFileNameWithoutExtension(asset.Directory.GetFiles("*.csproj").First().Name);
            string packageName = $"dotnettry.{name}";

            var console = new TestConsole();

            await PackCommand.Do(new PackOptions(asset.Directory, enableBlazor: true), console);

            asset.Directory
                 .GetFiles()
                 .Should()
                 .Contain(f => f.Name.Contains("nupkg"));

            var dotnet = new Dotnet(asset.Directory);

            var result = await dotnet.ToolInstall(packageName, asset.Directory, asset.Directory);

            var exe = Path.Combine(asset.Directory.FullName, packageName);

            result = await MLS.Agent.Tools.CommandLine.Execute(exe, "extract-package", workingDir: asset.Directory);
            result = await MLS.Agent.Tools.CommandLine.Execute(exe, "locate-projects", workingDir: asset.Directory);

            var projectDirectory = new DirectoryInfo(string.Join("", result.Output));
            var subDirectories = projectDirectory.GetDirectories();
            subDirectories.Should().Contain(d => d.Name == "packTarget");
            subDirectories.Should().Contain(d => d.Name == $"runner-{name}");

        }

    }
}
