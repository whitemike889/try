using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clockwise;
using MLS.Agent.Tools;
using WorkspaceServer.Packaging;

namespace WorkspaceServer.PackageDiscovery
{
    internal class GlobalToolPackageDiscoveryStrategy : IPackageDiscoveryStrategy
    {
        CustomPackageLocator _locator = new CustomPackageLocator();

        public async Task<PackageBuilder> Locate(string workspaceName, Budget budget)
        {
            var locatedPackage = await _locator.LocatePackageAsync(workspaceName, budget);
            if (locatedPackage != null)
            {
                var pb = new PackageBuilder(workspaceName, new GlobalToolInitializer(workspaceName));
                pb.Directory = locatedPackage.Directory;
                return pb;
            }

            return null;
        }

        class GlobalToolInitializer : IPackageInitializer
        {
            private readonly string _toolName;

            public GlobalToolInitializer(string toolName)
            {
                _toolName = toolName;
            }

            public Task Initialize(DirectoryInfo directory, Budget budget = null)
            {
                return CommandLine.Execute(_toolName, "extract-package", budget: budget);
            }
        }

        private class CustomPackageLocator
        {
            public async Task<Package> LocatePackageAsync(string name, Budget budget)
            {
                var result = await CommandLine.Execute(name, "locate-assembly", budget: budget);
                var output = result.Output.FirstOrDefault();
                if (output == null || !File.Exists(output))
                {
                    return null;
                }

                var directory = Path.GetDirectoryName(output);
                var projectDirectory = Path.Combine(directory, "project");
                Console.WriteLine($"Project: {projectDirectory}");
                var package = new Package(name, directory: new DirectoryInfo(projectDirectory));
                return package;
            }
        }
    }
}
