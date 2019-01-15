using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Clockwise;
using WorkspaceServer.Packaging;

namespace WorkspaceServer.PackageDiscovery
{
    internal partial class GlobalToolPackageDiscoveryStrategy : IPackageDiscoveryStrategy
    {
        CustomPackageLocator _locator = new CustomPackageLocator(null);

        public async Task<PackageBuilder> Locate(string workspaceName, Budget budget)
        {
            var locatedPackage = await _locator.LocatePackageAsync(workspaceName, budget);
            if (locatedPackage != null)
            {
                var pb = new PackageBuilder(workspaceName, new PackageToolInitializer(workspaceName, null));
                pb.Directory = locatedPackage.Directory;
                return pb;
            }

            return null;
        }
    }
}
