using MLS.Agent.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WorkspaceServer.Packaging;

namespace WorkspaceServer
{
    public class PrebuiltBlazorPackageLocator
    {
        private DirectoryInfo defaultPackagesDirectory;

        public PrebuiltBlazorPackageLocator(DirectoryInfo defaultPackagesDirectory)
        {
            this.defaultPackagesDirectory = defaultPackagesDirectory;
        }

        public async Task<IEnumerable<Package>> Discover()
        {
            var dotnet = new Dotnet(this.defaultPackagesDirectory);
            var tools = await dotnet.ToolList(defaultPackagesDirectory);

            var packages = new List<Package>();
            foreach (var tool in tools)
            {
                if (tool.StartsWith("dotnettry."))
                {
                    var result = await CommandLine.Execute(Path.Combine(defaultPackagesDirectory.FullName,  tool), "locate-projects");
                    var directory = new DirectoryInfo(result.Output.First());
                    if (directory.Exists)
                    {
                        var runnerSubDirectory = directory.GetDirectories("runner-*").FirstOrDefault();
                        if (runnerSubDirectory.Exists)
                        {
                            var path = Path.Combine(runnerSubDirectory.FullName, "MLS.Blazor");
                            var package = new BlazorPackage(runnerSubDirectory.Name, null, new DirectoryInfo(path));
                            if (package.BlazorEntryPointAssemblyPath.Exists)
                            {
                                packages.Add(package);
                            }
                        }
                    }
                }
            }

            return packages;
        }
    }
}