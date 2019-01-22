using System.IO;
using System.Threading.Tasks;
using Clockwise;
using WorkspaceServer.Packaging;

namespace WorkspaceServer.PackageDiscovery
{
    public class LocalToolPackageDiscoveryStrategy : IPackageDiscoveryStrategy
    {
        private readonly DirectoryInfo _workingDirectory;
        ToolPackageLocator _locator;
        private readonly DirectoryInfo _addSource;

        public LocalToolPackageDiscoveryStrategy(DirectoryInfo workingDirectory, DirectoryInfo addSource)
        { 
            _workingDirectory = workingDirectory;
            _locator = new ToolPackageLocator(workingDirectory.FullName);
            _addSource = addSource;
        }

        public async Task<PackageBuilder> Locate(PackageDescriptor packageDesciptor, Budget budget = null)
        {
            var locatedPackage = await _locator.LocatePackageAsync(packageDesciptor.Name, budget);
            if (locatedPackage != null)
            {
                var pb = new PackageBuilder(packageDesciptor.Name, 
                    new PackageToolInitializer(Path.Combine(_workingDirectory.FullName, packageDesciptor.Name), _workingDirectory));
                pb.Directory = locatedPackage.Directory;
                return pb;
            }

            return await TryInstallAndLocateTool(packageDesciptor, budget);
        }

        private async Task<PackageBuilder> TryInstallAndLocateTool(PackageDescriptor packageDesciptor, Budget budget)
        {
            var dotnet = new Dotnet();
            var installationResult = await dotnet.ToolInstall(
                packageDesciptor.Name,
                _workingDirectory.FullName,
                _addSource);

            if (installationResult.ExitCode != 0)
            {
                return null;
            }

            var tool = await _locator.LocatePackageAsync(packageDesciptor.Name, budget);
            if (tool != null)
            {
                var pb = new PackageBuilder(packageDesciptor.Name,
                    new PackageToolInitializer(Path.Combine(_workingDirectory.FullName, packageDesciptor.Name), _workingDirectory));
                pb.Directory = tool.Directory;
                return pb;
            }

            return null;
        }
    }
}
