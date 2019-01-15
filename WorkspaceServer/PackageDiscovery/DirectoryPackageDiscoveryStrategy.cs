using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Clockwise;
using WorkspaceServer.Packaging;

namespace WorkspaceServer.PackageDiscovery
{
    internal class DirectoryPackageDiscoveryStrategy : IPackageDiscoveryStrategy
    {
        public Task<PackageBuilder> Locate(PackageDescriptor packageDescriptor, Budget budget)
        {
            var directory = new DirectoryInfo(Path.Combine(
                    Package.DefaultPackagesDirectory.FullName, packageDescriptor.Name));

            if (directory.Exists)
            {
                return Task.FromResult(new PackageBuilder(packageDescriptor.Name));
            }

            return Task.FromResult<PackageBuilder>(null);
        }
    }
}
