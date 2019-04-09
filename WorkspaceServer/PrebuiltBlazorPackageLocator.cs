using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MLS.Agent.Tools;
using Pocket;
using WorkspaceServer.Packaging;
using static Pocket.Logger<WorkspaceServer.PrebuiltBlazorPackageLocator>;

namespace WorkspaceServer
{
    public class PrebuiltBlazorPackageLocator
    {
        private readonly DirectoryInfo _packagesDirectory;

        public PrebuiltBlazorPackageLocator(DirectoryInfo packagesDirectory = null)
        {
            _packagesDirectory = packagesDirectory ?? Package.DefaultPackagesDirectory;
        }

        public async Task<BlazorPackage> Locate(string name)
        {
            using (var operation = Log.OnEnterAndExit())
            {
                var dotnet = new Dotnet(_packagesDirectory);
                var toolNames = await dotnet.ToolList(_packagesDirectory);

                var packages = new List<BlazorPackage>();

                foreach (var toolName in toolNames)
                {
                    if (toolName.StartsWith("dotnettry."))
                    {
                        operation.Info($"Checking tool {toolName}");
                        var result = await CommandLine.Execute(Path.Combine(_packagesDirectory.FullName, toolName), "locate-projects");
                        var directory = new DirectoryInfo(result.Output.First());
                        if (directory.Exists)
                        {
                            var runnerSubDirectory = directory.GetDirectories("runner-*").FirstOrDefault();
                            if (runnerSubDirectory?.Exists ?? false)
                            {
                                var path = Path.Combine(runnerSubDirectory.FullName, "MLS.Blazor");
                                var package = new BlazorPackage(runnerSubDirectory.Name, null, new DirectoryInfo(path));
                                if (package.BlazorEntryPointAssemblyPath.Exists)
                                {
                                    packages.Add(package);
                                    return package;
                                }
                            }
                        }
                    }
                }

                return null;
            }
        }
    }
}