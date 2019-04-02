using Xunit;
using WorkspaceServer.PackageDiscovery;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MLS.Agent.Tests.TestUtility;
using WorkspaceServer;
using WorkspaceServer.Tests;

namespace MLS.Agent.Tests
{
    public class ProjectFilePackageDiscoveryStrategyTests
    {
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
