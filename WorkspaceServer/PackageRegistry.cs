using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Clockwise;
using WorkspaceServer.PackageDiscovery;
using WorkspaceServer.Packaging;

namespace WorkspaceServer
{
    public partial class PackageRegistry : IEnumerable<Task<PackageBuilder>>
    {
        private readonly ConcurrentDictionary<string, Task<PackageBuilder>> _packageBuilders = new ConcurrentDictionary<string, Task<PackageBuilder>>();
        private readonly IEnumerable<IPackageDiscoveryStrategy> _strategies;

        public PackageRegistry()
        {
            _strategies = new IPackageDiscoveryStrategy[] 
            {
                new DirectoryPackageDiscoveryStrategy(),
                new GlobalToolPackageDiscoveryStrategy()
            };
        }

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

        public async Task<Package> Get(string packageName, Budget budget = null)
        {
            if (packageName == "script")
            {
                packageName = "console";
            }

            var build = await (await _packageBuilders.GetOrAdd(
                            packageName,
                            async name =>
                            {
                                foreach (var strategy in _strategies)
                                {
                                    var builder = await strategy.Locate(new PackageDescriptor(packageName), budget);
                                    if (builder != null)
                                    {
                                        return builder;
                                    }
                                }

                                throw new ArgumentException($"Workspace named \"{name}\" not found.");
                            })).GetPackage(budget);


            await build.EnsureReady(budget);
            
            return build;
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
