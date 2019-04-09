using MLS.Agent.Tools;
using Pocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WorkspaceServer.Packaging;
using static Pocket.Logger<WorkspaceServer.PrebuiltBlazorPackageLocator>;

namespace WorkspaceServer
{
    public class PrebuiltBlazorPackageLocator
    {
        private readonly DirectoryInfo _packagesDirectory;

        public PrebuiltBlazorPackageLocator(DirectoryInfo packagesDirectory)
        {
            _packagesDirectory = packagesDirectory ?? throw new ArgumentNullException(nameof(packagesDirectory));
        }

        public async Task<IEnumerable<BlazorPackage>> Discover()
        {
            using (var operation = Log.OnEnterAndConfirmOnExit())
            {
                var dotnet = new Dotnet(_packagesDirectory);
                var tools = await dotnet.ToolList(_packagesDirectory);

                var packages = new List<BlazorPackage>();
                foreach (var tool in tools)
                {
                    if (tool.StartsWith("dotnettry."))
                    {
                        operation.Info($"Checking tool {tool}");
                        var result = await CommandLine.Execute(Path.Combine(_packagesDirectory.FullName, tool), "locate-projects");
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
                                    operation.Info($"Adding package {package}");
                                    packages.Add(package);
                                }
                            }
                            else
                            {
                                operation.Info($"Rejected package {tool}");
                            }
                        }
                    }
                }

                operation.Succeed();
                return packages;
            }
        }
    }
}