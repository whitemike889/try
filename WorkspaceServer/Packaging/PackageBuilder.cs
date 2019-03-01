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
            _afterCreateActions.Add(async (workspace, budget) =>
            {
                var dotnet = new Dotnet(workspace.Directory);
                await dotnet.AddPackage(packageId, version);
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

        public async Task<Package> GetPackage(Budget budget = null)
        {
            if (package == null)
            {
                await PreparePackage(budget);
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

        private async Task PreparePackage(Budget budget = null)
        {
            budget = budget ?? new Budget();

            if (CreateRebuildablePackage)
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

            await package.EnsureReady(budget);

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
