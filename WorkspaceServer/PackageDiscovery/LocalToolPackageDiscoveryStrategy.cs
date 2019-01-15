using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Clockwise;
using WorkspaceServer.Packaging;

namespace WorkspaceServer.PackageDiscovery
{
    public class LocalToolPackageDiscoveryStrategy : IPackageDiscoveryStrategy
    {
        private readonly DirectoryInfo _workingDirectory;
        CustomPackageLocator _locator;

        public LocalToolPackageDiscoveryStrategy(DirectoryInfo workingDirectory)
        {
            _workingDirectory = workingDirectory;
            _locator = new CustomPackageLocator(workingDirectory);
        }

        public async Task<PackageBuilder> Locate(string workspaceName, Budget budget = null)
        {
            var locatedPackage = await _locator.LocatePackageAsync(workspaceName, budget);
            if (locatedPackage != null)
            {
                var pb = new PackageBuilder(workspaceName, new PackageToolInitializer(workspaceName, _workingDirectory));
                pb.Directory = locatedPackage.Directory;
                return pb;
            }

            return null;
        }
    }
}
