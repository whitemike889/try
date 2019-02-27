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
    public class PackageRegistry : IEnumerable<Task<PackageBuilder>>
    {
        private readonly ConcurrentDictionary<string, Task<PackageBuilder>> _packageBuilders = new ConcurrentDictionary<string, Task<PackageBuilder>>();
        private readonly IEnumerable<IPackageDiscoveryStrategy> _strategies;
        private bool _createRebuildablePackage;

        public PackageRegistry(
            bool createRebuildablePackage = false,
            params IPackageDiscoveryStrategy[] additionalStrategies)
            : this(createRebuildablePackage, new IPackageDiscoveryStrategy[]
            {
                new ProjectFilePackageDiscoveryStrategy(),
                new DirectoryPackageDiscoveryStrategy(),
                new GlobalToolPackageDiscoveryStrategy(),
            }.Concat(additionalStrategies))
        {
        }

        private PackageRegistry(bool createRebuildablePackage, IEnumerable<IPackageDiscoveryStrategy> strategies)
        {
            _createRebuildablePackage = createRebuildablePackage;
            _strategies = strategies;
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

            var packageBuilder = new PackageBuilder(name);
            configure(packageBuilder);
            _packageBuilders.TryAdd(name, Task.FromResult(packageBuilder));
        }
      
        public async Task<Package> Get(string packageName, Budget budget = null)
        {
            if (packageName == "script")
            {
                packageName = "console";
            }

            var package = await (await _packageBuilders.GetOrAdd(
                            packageName,
                            async name =>
                            {
                                foreach (var strategy in _strategies)
                                {
                                    var builder = await strategy.Locate(new PackageDescriptor(packageName, _createRebuildablePackage), budget);
                                    if (builder != null)
                                    {
                                        return builder;
                                    }
                                }

                                throw new ArgumentException($"Package named \"{name}\" not found.");
                            })).GetPackage(budget);


            await package.EnsureReady(budget);

            return package;
        }

        public IEnumerable<Task<PackageInfo>> GetRegisteredPackageInfos()
        {
            var packageInfos = _packageBuilders?.Values.Select(async wb => (await wb).GetPackageInfo()).Where(info => info != null).ToArray() ?? Array.Empty<Task<PackageInfo>>();

            return packageInfos;
        }

        public static PackageRegistry CreateForVerifyMode(DirectoryInfo project, DirectoryInfo addSource)
        {
            var registry = new PackageRegistry(true,
               new LocalToolPackageDiscoveryStrategy(Package.DefaultPackagesDirectory, addSource));

            registry.Add(project.Name, builder =>
            {
                builder.CreateRebuildablePackage = true;
                builder.Directory = project;
            });

            return registry;
        }

        public static PackageRegistry CreateForTryMode(DirectoryInfo project, DirectoryInfo addSource)
        {
            var registry = new PackageRegistry(false,
                new LocalToolPackageDiscoveryStrategy(Package.DefaultPackagesDirectory, addSource));

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
