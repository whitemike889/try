using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Clockwise;
using MLS.Agent.Tools;
using WorkspaceServer.Packaging;

namespace WorkspaceServer
{
    public class PackageRegistry : IEnumerable<Task<PackageBuilder>>
    {
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

        private readonly CustomPackageLocator _locator = new CustomPackageLocator();
        private readonly ConcurrentDictionary<string, Task<PackageBuilder>> _packageBuilders = new ConcurrentDictionary<string, Task<PackageBuilder>>();

        public void Add(string name, Action<PackageBuilder> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            }

            var options = new PackageBuilder(name);
            configure(options);
            _packageBuilders.TryAdd(name, Task.FromResult(options));
        }

        public async Task<Package> Get(string workspaceName, Budget budget = null)
        {
            if (workspaceName == "script")
            {
                workspaceName = "console";
            }

            var build = await (await _packageBuilders.GetOrAdd(
                            workspaceName,
                            async name =>
                            {
                                var directory = new DirectoryInfo(
                                    Path.Combine(
                                        Package.DefaultPackagesDirectory.FullName, workspaceName));

                                if (directory.Exists)
                                {
                                    return new PackageBuilder(name);
                                }

                                var locatedPackage = await _locator.LocatePackageAsync(name, budget);
                                if (locatedPackage != null)
                                {
                                    var pb = new PackageBuilder(name, new GlobalToolInitializer(name));
                                    pb.Directory = locatedPackage.Directory;
                                    return pb;
                                }


                                throw new ArgumentException($"Workspace named \"{name}\" not found.");
                            })).GetPackage(budget);


            await build.EnsureReady(budget);
            
            return build;
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

        public IEnumerable<Task<PackageInfo>> GetRegisteredPackageInfos()
        {
            var workspaceInfos = _packageBuilders?.Values.Select(async wb => (await wb).GetPackageInfo()).Where(info => info != null).ToArray() ?? Array.Empty<Task<PackageInfo>>();

            return workspaceInfos;
        }

        public static PackageRegistry CreateForTryMode(DirectoryInfo project)
        {
            var registry = new PackageRegistry();

            registry.Add(project.Name, builder =>
            {
                builder.Directory = project;
            });

            return registry;
        }

        public static PackageRegistry CreateForHostedMode()
        {
            var registry = new PackageRegistry();

            registry.Add("console",
                         workspace =>
                         {
                             workspace.CreateUsingDotnet("console");
                             workspace.AddPackageReference("Newtonsoft.Json");
                         });

            registry.Add("nodatime.api",
                         workspace =>
                         {
                             workspace.CreateUsingDotnet("console");
                             workspace.AddPackageReference("NodaTime", "2.3.0");
                             workspace.AddPackageReference("NodaTime.Testing", "2.3.0");
                             workspace.AddPackageReference("Newtonsoft.Json");
                         });

            registry.Add("aspnet.webapi",
                         workspace =>
                         {
                             workspace.CreateUsingDotnet("webapi");
                             workspace.RequiresPublish = true;
                         });

            registry.Add("xunit",
                         workspace =>
                         {
                             workspace.CreateUsingDotnet("xunit", "tests");
                             workspace.AddPackageReference("Newtonsoft.Json");
                             workspace.DeleteFile("UnitTest1.cs");
                         });

            registry.Add("blazor-console",
                         workspace =>
                         {
                             workspace.CreateUsingDotnet("classlib");
                             workspace.AddPackageReference("Newtonsoft.Json");
                         });

            registry.Add("blazor-nodatime",
                         workspace =>
                         {
                             workspace.CreateUsingDotnet("classlib");
                             workspace.AddPackageReference("NodaTime", "2.3.0");
                             workspace.AddPackageReference("NodaTime.Testing", "2.3.0");
                             workspace.AddPackageReference("Newtonsoft.Json");
                         });

            return registry;
        }

        public IEnumerator<Task<PackageBuilder>> GetEnumerator() =>
            _packageBuilders.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}
