using System;
using System.CommandLine;
using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;
using MLS.Agent.CommandLine;
using Pocket;
using Xunit;
using Xunit.Abstractions;

namespace WorkspaceServer.Tests
{
    public class PrebuiltBlazorPackageLocatorTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly IDisposable _disposables;

        public PrebuiltBlazorPackageLocatorTests(ITestOutputHelper output)
        {
            _output = output;
            _disposables = output.SubscribeToPocketLogger();
        }

        [Fact]
        public async Task Discovers_built_blazor_package()
        {
            var (packageName, addSource) = await Create.NupkgWithBlazorEnabled();
            var options = new InstallOptions(addSource, packageName);
            await InstallCommand.Do(options, new TestConsole());
            var locator = new PrebuiltBlazorPackageLocator();

            var package = await locator.Locate(packageName);

            package.Name.Should().Be($"runner-{packageName}");
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}