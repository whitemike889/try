using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Clockwise;
using WorkspaceServer.Packaging;

namespace WorkspaceServer.PackageDiscovery
{
    internal partial class GlobalToolPackageDiscoveryStrategy : IPackageDiscoveryStrategy
    {
        ToolPackageLocator _locator = new ToolPackageLocator("");

        public async Task<PackageBuilder> Locate(PackageDescriptor packageDescriptor, Budget budget)
        {
            var locatedPackage = await _locator.LocatePackageAsync(packageDescriptor.Name, budget);
            if (locatedPackage != null)
            {
                var pb = new PackageBuilder(packageDescriptor.Name, new PackageToolInitializer(packageDescriptor.Name, null));
                pb.Directory = locatedPackage.Directory;
                return pb;
            }

            return null;
        }
    }
}
