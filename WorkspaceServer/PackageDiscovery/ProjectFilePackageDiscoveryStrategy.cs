using System.IO;
using System.Threading.Tasks;
using Clockwise;
using WorkspaceServer.Packaging;

namespace WorkspaceServer.PackageDiscovery
{
    public class ProjectFilePackageDiscoveryStrategy : IPackageDiscoveryStrategy
    {
        public Task<PackageBuilder> Locate(PackageDescriptor packageDescriptor, Budget budget = null)
        {
            var projectFile = packageDescriptor.Name;

            if(Path.GetExtension(projectFile) == ".csproj" && File.Exists(projectFile))
            {
                PackageBuilder packageBuilder = new PackageBuilder(packageDescriptor.Name);
                packageBuilder.Directory = new DirectoryInfo(Path.GetDirectoryName(projectFile));
                return Task.FromResult(packageBuilder);
            }

            return Task.FromResult<PackageBuilder>(null);
        }
    }
}
