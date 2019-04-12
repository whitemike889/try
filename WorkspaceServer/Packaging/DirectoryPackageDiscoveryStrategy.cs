using System;
using System.IO;
using System.Threading.Tasks;
using Clockwise;

namespace WorkspaceServer.Packaging
{
    internal class DirectoryPackageDiscoveryStrategy : IPackageDiscoveryStrategy
    {
        public Task<PackageBuilder> Locate(PackageDescriptor packageDescriptor, Budget budget)
        {
            var directory = new DirectoryInfo(Path.Combine(
                    Package.DefaultPackagesDirectory.FullName, packageDescriptor.Name));

            if (directory.Exists)
            {
                var packageBuilder = new PackageBuilder(packageDescriptor.Name);
                packageBuilder.CreateRebuildablePackage = packageDescriptor.IsRebuildable;
                return Task.FromResult(packageBuilder);
            }

            return Task.FromResult<PackageBuilder>(null);
        }
    }
}
