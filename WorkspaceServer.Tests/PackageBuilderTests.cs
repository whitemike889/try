using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using WorkspaceServer.Packaging;
using Xunit;

namespace WorkspaceServer.Tests
{
    public class PackageBuilderTests
    {
        [Fact]
        public async Task EnableBlazor_registers_another_package()
        {
            var builder = new PackageBuilder("test");
            var registry = new PackageRegistry();

            builder.EnableBlazor(registry);

            var addedBuilder = registry.First(t =>
                t.Result.PackageName == "runner-test").Result;

            addedBuilder.BlazorSupported.Should().BeTrue();


            var package = await registry.Get("runner-test");
            package.Should().NotBeNull();

            package.Directory.Name.Should().Be("MLS.Blazor");
        }
    }
}