// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Threading.Tasks;
using Clockwise;
using Pocket;

namespace WorkspaceServer.Packaging
{
    public class LocalToolInstallingPackageDiscoveryStrategy : IPackageFinder
    {
        private readonly DirectoryInfo _workingDirectory;
        private readonly ToolPackageLocator _locator;
        private readonly DirectoryInfo _addSource;

        public LocalToolInstallingPackageDiscoveryStrategy(DirectoryInfo workingDirectory, DirectoryInfo addSource = null)
        { 
            _workingDirectory = workingDirectory;
            _locator = new ToolPackageLocator(workingDirectory);
            _addSource = addSource;
        }

        private async Task<IPackage> TryInstallAndLocateTool(PackageDescriptor packageDesciptor, Budget budget)
        {
            var dotnet = new Dotnet();

            var installationResult = await dotnet.ToolInstall(
                packageDesciptor.Name,
                _workingDirectory,
                _addSource,
                budget);

            if (installationResult.ExitCode != 0)
            {
                Logger<LocalToolInstallingPackageDiscoveryStrategy>.Log.Warning($"Tool not installed: {packageDesciptor.Name}");
                return null;
            }

            var tool = await _locator.LocatePackageAsync(packageDesciptor.Name, budget);

            return tool;
        }

        public async Task<T> Find<T>(PackageDescriptor descriptor) where T : class, IPackage
        {
            var locatedPackage = await _locator.LocatePackageAsync(descriptor.Name, new Budget());
            if (locatedPackage != null)
            {
                return locatedPackage as T;
            }

            return (await TryInstallAndLocateTool(descriptor, new Budget())) as T;
        }
    }
}
