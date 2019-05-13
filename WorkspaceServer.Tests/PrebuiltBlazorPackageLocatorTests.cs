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
            await InstallCommand.Do(new InstallOptions(addSource, packageName), new TestConsole());
            var locator = new PrebuiltBlazorPackageLocator();

            var asset = await locator.Locate(packageName);
            asset.DirectoryAccessor.FileExists("MLS.Blazor.dll").Should().Be(true);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}