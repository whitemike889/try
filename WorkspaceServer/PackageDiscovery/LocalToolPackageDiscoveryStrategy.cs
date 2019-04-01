using System;
using System.IO;
using System.Threading.Tasks;
using Clockwise;
using WorkspaceServer.Packaging;

namespace WorkspaceServer.PackageDiscovery
{
    public class LocalToolPackageDiscoveryStrategy : IPackageDiscoveryStrategy
    {
        private readonly DirectoryInfo _workingDirectory;
        private readonly ToolPackageLocator _locator;
        private readonly DirectoryInfo _addSource;

        public LocalToolPackageDiscoveryStrategy(DirectoryInfo workingDirectory, DirectoryInfo addSource = null)
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
                return CreatePackageBuilder(packageDesciptor, locatedPackage);
            }

            return await TryInstallAndLocateTool(packageDesciptor, budget);
        }

        private async Task<PackageBuilder> TryInstallAndLocateTool(PackageDescriptor packageDesciptor, Budget budget)
        {
            var dotnet = new Dotnet();
            var installationResult = await dotnet.ToolInstall(
                packageDesciptor.Name,
                _workingDirectory,
                _addSource,
                budget);

            if (installationResult.ExitCode != 0)
            {
                Console.WriteLine($"Tool not installed: {packageDesciptor.Name}");
                return null;
            }

            var tool = await _locator.LocatePackageAsync(packageDesciptor.Name, budget);

            if (tool != null)
            {
                return CreatePackageBuilder(packageDesciptor, tool);
            }

            return null;
        }

        private PackageBuilder CreatePackageBuilder(PackageDescriptor packageDesciptor, Package locatedPackage)
        {
            var pb = new PackageBuilder(
                packageDesciptor.Name,
                new PackageToolInitializer(
                    Path.Combine(
                        _workingDirectory.FullName, packageDesciptor.Name),
                    _workingDirectory));
            pb.Directory = locatedPackage.Directory;
            return pb;
        }
    }
}
