using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Clockwise;

namespace WorkspaceServer.Packaging
{
    public class PackageBuilder
    {
        private Package package;
        private readonly List<Func<Package, Budget, Task>> _afterCreateActions = new List<Func<Package, Budget, Task>>();
        private readonly List<string> _addPackages = new List<string>();

        public PackageBuilder(string packageName, IPackageInitializer packageInitializer = null)
        {
            if (string.IsNullOrWhiteSpace(packageName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(packageName));
            }

            PackageName = packageName;
            PackageInitializer = packageInitializer;
        }

        public string PackageName { get; }

        public IPackageInitializer PackageInitializer { get; private set; }

        public DirectoryInfo Directory { get; set; }
        public bool CreateRebuildablePackage { get; internal set; }
        public bool BlazorSupported { get; private set; }

        public void AfterCreate(Func<Package, Budget, Task> action)
        {
            _afterCreateActions.Add(action);
        }

        public void CreateUsingDotnet(string template, string projectName = null)
        {
            PackageInitializer = new PackageInitializer(
               template,
               projectName ?? PackageName,
               AfterCreate);
        }

        public void AddPackageReference(string packageId, string version = null)
        {
            _addPackages.Add(packageId);
            _afterCreateActions.Add(async (package, budget) =>
            {
                Func<Task> action = async () =>
                {
                    var dotnet = new Dotnet(package.Directory);
                    await dotnet.AddPackage(packageId, version);
                };

                await action();
               
            });
        }

        public void EnableBlazor(PackageRegistry registry)
        {
            if (this.BlazorSupported)
            {
                throw new Exception($"Package {this.PackageName} is already a blazor package");
            }

            var name = $"runner-{this.PackageName}";
            registry.Add(name, pb =>
            {

                var initializer = new BlazorPackageInitializer(this.PackageName, _addPackages);
                pb.PackageInitializer = initializer;
                pb.BlazorSupported = true;
                pb.Directory = new DirectoryInfo(Path.Combine(Package.DefaultPackagesDirectory.FullName, pb.PackageName, "MLS.Blazor"));
            });
        }

        public void DeleteFile(string relativePath)
        {
            _afterCreateActions.Add(async (workspace, budget) =>
            {
                await Task.Yield();
                var filePath = Path.Combine(workspace.Directory.FullName, relativePath);
                File.Delete(filePath);
            });
        }

        public Package GetPackage(Budget budget = null)
        {
            if (package == null)
            {
                PreparePackage(budget);
            }

            budget?.RecordEntry();
            return package;
        }

        public PackageInfo GetPackageInfo()
        {
            PackageInfo info = null;
            if (package != null)
            {
                info = new PackageInfo(
                    package.Name,
                    package.LastSuccessfulBuildTime,
                    package.ConstructionTime,
                    package.PublicationTime,
                    package.CreationTime,
                    package.ReadyTime
                );
            }

            return info;
        }

        private void PreparePackage(Budget budget = null)
        {
            budget = budget ?? new Budget();

            if (PackageInitializer is BlazorPackageInitializer)
            {
                package = new BlazorPackage(
                        PackageName,
                        PackageInitializer,
                        Directory);
            }
            else if (CreateRebuildablePackage)
            {
                package = new RebuildablePackage(
                        PackageName,
                        PackageInitializer,
                        Directory);
            }
            else
            {
                package = new NonrebuildablePackage(
                        PackageName,
                        PackageInitializer,
                        Directory);
            }

            budget.RecordEntry();
        }

        private async Task AfterCreate(DirectoryInfo directoryInfo, Budget budget)
        {
            foreach (var action in _afterCreateActions)
            {
                await action(package, budget);
            }
        }
    }
}
