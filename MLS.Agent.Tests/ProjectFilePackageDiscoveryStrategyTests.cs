using Xunit;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using WorkspaceServer;
using WorkspaceServer.Packaging;
using WorkspaceServer.Tests;
using Xunit.Abstractions;

namespace MLS.Agent.Tests
{
    public class ProjectFilePackageDiscoveryStrategyTests
    {   private readonly ITestOutputHelper output;

        public ProjectFilePackageDiscoveryStrategyTests(ITestOutputHelper _output)
        {
            output = _output;
        }

        [Fact]
        public async Task Discover_package_from_project_file()
        {
            var strategy = new ProjectFilePackageDiscoveryStrategy();
            var sampleProject = (await Create.ConsoleWorkspaceCopy()).Directory;
            var projectFile = sampleProject.GetFiles("*.csproj").Single();
            var packageBuilder = await strategy.Locate(new PackageDescriptor(projectFile.FullName));

            packageBuilder.PackageName.Should().Be(projectFile.FullName);
            packageBuilder.Directory.FullName.Should().Be(sampleProject.FullName);
        }
    }
}
