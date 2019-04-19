using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Clockwise;
using WorkspaceServer.Packaging;

namespace WorkspaceServer
{
    public class PackageRegistry : IEnumerable<Task<PackageBuilder>>
    {
        private readonly List<IPackageFinder> _packageFinders;

        private readonly ConcurrentDictionary<string, Task<PackageBuilder>> _packageBuilders = new ConcurrentDictionary<string, Task<PackageBuilder>>();
        private readonly ConcurrentDictionary<string, Task<IPackage>> _packages = new ConcurrentDictionary<string, Task<IPackage>>();
        private readonly List<IPackageDiscoveryStrategy> _strategies = new List<IPackageDiscoveryStrategy>();
        private readonly bool _createRebuildablePackage;

        public PackageRegistry(
            bool createRebuildablePackage = false,
            params IPackageDiscoveryStrategy[] additionalStrategies)
            : this(createRebuildablePackage, new IPackageDiscoveryStrategy[]
            {
                new ProjectFilePackageDiscoveryStrategy(),
                new DirectoryPackageDiscoveryStrategy()
            }.Concat(additionalStrategies))
        {
        }

        private PackageRegistry(
            bool createRebuildablePackage, 
            IEnumerable<IPackageDiscoveryStrategy> strategies,
            IEnumerable<IPackageFinder> packageFinders = null)
        {
            _createRebuildablePackage = createRebuildablePackage;

            foreach (var strategy in strategies)
            {
                if (strategy == null)
                {
                    throw new ArgumentException($"Strategy cannot be null.");
                }

                _strategies.Add(strategy);
            }

            _packageFinders = packageFinders?.ToList() ?? GetDefaultPackageFinders().ToList();
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

        public async Task<T> Get<T>(string packageName, Budget budget = null)
            where T : IPackage
        {
            if (packageName == "script")
            {
                packageName = "console";
            }

            var package = await _packages.GetOrAdd(packageName, async name =>
            {
                var descriptor = new PackageDescriptor(packageName);

                foreach (var packgeFinder in _packageFinders)
                {
                    if (await packgeFinder.Find<T>(descriptor) is T pkg)
                    {
                        return pkg;
                    }
                }

                var packageBuilder = await _packageBuilders.GetOrAdd(
                    name,
                    async name2 =>
                    {
                        var packageDescriptor = new PackageDescriptor(name2, _createRebuildablePackage);

                        foreach (var strategy in _strategies)
                        {
                            var builder = await strategy.Locate(packageDescriptor, budget);

                            if (builder != null)
                            {
                                return builder;
                            }
                        }

                        throw new PackageNotFoundException($"Package named \"{name2}\" not found.");
                    });

                return packageBuilder.GetPackage(budget);
            });

            return (T) package;
        }

        public static PackageRegistry CreateForTryMode(DirectoryInfo project, DirectoryInfo addSource = null)
        {
            var registry = new PackageRegistry(true,
               new LocalToolInstallingPackageDiscoveryStrategy(Package.DefaultPackagesDirectory, addSource));

            registry.Add(project.Name, builder =>
            {
                builder.CreateRebuildablePackage = true;
                builder.Directory = project;
            });

            return registry;
        }

        public static PackageRegistry CreateForHostedMode()
        {
            var registry = new PackageRegistry(false, new LocalToolInstallingPackageDiscoveryStrategy(Package.DefaultPackagesDirectory));

            registry.Add("console",
                         packageBuilder =>
                         {
                             packageBuilder.CreateUsingDotnet("console");
                             packageBuilder.TrySetLanguageVersion("8.0");
                             packageBuilder.AddPackageReference("Newtonsoft.Json");
                         });

            registry.Add("nodatime.api",
                         packageBuilder =>
                         {
                             packageBuilder.CreateUsingDotnet("console");
                             packageBuilder.TrySetLanguageVersion("8.0");
                             packageBuilder.AddPackageReference("NodaTime", "2.3.0");
                             packageBuilder.AddPackageReference("NodaTime.Testing", "2.3.0");
                             packageBuilder.AddPackageReference("Newtonsoft.Json");
                         });

            registry.Add("aspnet.webapi",
                         packageBuilder =>
                         {
                             packageBuilder.CreateUsingDotnet("webapi");
                             packageBuilder.TrySetLanguageVersion("8.0");
                         });

            registry.Add("xunit",
                         packageBuilder =>
                         {
                             packageBuilder.CreateUsingDotnet("xunit", "tests");
                             packageBuilder.TrySetLanguageVersion("8.0");
                             packageBuilder.AddPackageReference("Newtonsoft.Json");
                             packageBuilder.DeleteFile("UnitTest1.cs");
                         });

            registry.Add("blazor-console",
                         packageBuilder =>
                         {
                             packageBuilder.CreateUsingDotnet("classlib");
                             packageBuilder.AddPackageReference("Newtonsoft.Json");
                         });

            registry.Add("humanizer.api",
                         packageBuilder =>
                         {
                             packageBuilder.CreateUsingDotnet("classlib");
                             packageBuilder.AddPackageReference("Newtonsoft.Json");
                             packageBuilder.AddPackageReference("Humanizer");
                         });

            // Todo: soemething about nodatime 2.3 makes blazor toolchain fail to build
            registry.Add("blazor-nodatime.api",
                         packageBuilder =>
                         {
                             packageBuilder.CreateUsingDotnet("classlib");
                             packageBuilder.DeleteFile("Class1.cs");
                             packageBuilder.AddPackageReference("NodaTime", "2.4.4");
                             packageBuilder.AddPackageReference("NodaTime.Testing", "2.4.4");
                             packageBuilder.AddPackageReference("Newtonsoft.Json");
                             packageBuilder.EnableBlazor(registry);
                         });
                         
            registry.Add("blazor-ms.logging",
                         packageBuilder =>
                         {
                             packageBuilder.CreateUsingDotnet("classlib");
                             packageBuilder.DeleteFile("Class1.cs");
                             packageBuilder.AddPackageReference("Microsoft.Extensions.Logging", "2.2.0");
                             packageBuilder.EnableBlazor(registry);
                         });

            return registry;
        }

        public IEnumerator<Task<PackageBuilder>> GetEnumerator() =>
            _packageBuilders.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();

        private static IEnumerable<IPackageFinder> GetDefaultPackageFinders()
        {
            yield return new FindPackageInDefaultLocation(new FileSystemDirectoryAccessor(Package.DefaultPackagesDirectory));
        }
    }
}
