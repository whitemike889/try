using System.Threading.Tasks;
using FluentAssertions;
using WorkspaceServer.Packaging;
using Xunit;

namespace WorkspaceServer.Tests
{
    public class PackageRegistryTests 
    {
        private readonly PackageRegistry registry = new PackageRegistry();

      
        [Fact]
        public async Task PackageRegistry_will_return_same_instance_of_a_package()
        {
            var packageName = Package.CreateDirectory(nameof(PackageRegistry_will_return_same_instance_of_a_package)).Name;

            registry.Add(packageName,
                options => options.CreatePackageInitializer("console"));

            var package1 = await registry.Get(packageName);
            var package2 = await registry.Get(packageName);

            package1.Should().BeSameAs(package2);
        }
    }
}