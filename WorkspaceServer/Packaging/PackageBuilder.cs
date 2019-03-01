using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Clockwise;
using MLS.Agent.Tools;

namespace WorkspaceServer.Packaging
{
    public class PackageBuilder
    {
        private Package package;
        private bool _blazorEnabled;
        private readonly List<Func<Package, Budget, Task>> _afterCreateActions = new List<Func<Package, Budget, Task>>();
        private readonly List<Func<Task>> _addPackages = new List<Func<Task>>();

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
            _afterCreateActions.Add(async (workspace, budget) =>
            {
                Func<Task> action = async () =>
                {
                    var dotnet = new Dotnet(workspace.Directory);
                    await dotnet.AddPackage(packageId, version);
                };

                _addPackages.Add(action);
                await action();
               
            });
        }

        public void EnableBlazor()
        {
            BlazorSupported = true;

            _afterCreateActions.Add(async (package, budget) =>
            {
                var name = $"blazor-{package.Name}";
                var directory = package.Directory.Parent;
                var subdir = directory.CreateSubdirectory(name);

                var entryPoint = await MakeBlazorProject(budget, subdir);
                package.BlazorEntryPointAssemblyPath = new FileInfo(entryPoint);
            });
        }

        private async Task<string> MakeBlazorProject(Budget budget, DirectoryInfo directory)
        {
            directory = directory.CreateSubdirectory("MLS.Blazor");
            var initializer = new PackageInitializer("blazor", "MLS.Blazor");
            await initializer.Initialize(directory, budget);

            var dotnet = new Dotnet(directory);
            var root = directory.FullName;

            var toDelete = new[] { "Pages", "Shared", "wwwroot", "_ViewImports.cshtml" };
            foreach (var thing in toDelete)
            {
                var path = Path.Combine(root, thing);
                Delete(path);
            }

            var wwwRootFiles = new[] { "index.html", "interop.js" };
            var pagesFiles = new[] { "Index.cshtml", "Index.cshtml.cs" };
            var rootFiles = new[] { "Program.cs", "Startup.cs" };

            WriteAll(wwwRootFiles, "wwwroot", root);
            WriteAll(pagesFiles, "Pages", root);
            WriteAll(rootFiles, "", root);

            var result = await dotnet.AddPackage("MLS.WasmCodeRunner", "1.0.7880001-alpha-c895bf25");
            result.ThrowOnFailure();

            foreach (var addPackage in _addPackages)
            {
                await addPackage();
            }

            result = await dotnet.AddPackage("NodaTime");
            result = await dotnet.AddPackage("NodaTime.Testing");
            result.ThrowOnFailure();

            result = await dotnet.Build("");
            result.ThrowOnFailure();

            return Path.Combine(directory.FullName,
                "bin\\Debug\\netstandard2.0\\MLS.Blazor.dll");
        }

        private static void WriteAll(string[] resources, string targetDirectory, string root)
        {
            foreach (var resource in resources)
            {
                WriteResource(resource, targetDirectory, root);
            }
        }

        private static void WriteResource(string resourceName, string targetDirectory, string root)
        {
            var text = ReadManifestResource(resourceName);
            var directory = Path.Combine(root, targetDirectory);
            System.IO.Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, resourceName);
            File.WriteAllText(path, text);
        }

        private static string ReadManifestResource(string resourceName)
        {
            var assembly = typeof(PackageBuilder).Assembly;
            var resoures = assembly.GetManifestResourceNames();
            using (var reader = new StreamReader(assembly.GetManifestResourceStream($"WorkspaceServer.{resourceName}")))
            {
                return reader.ReadToEnd();
            }
        }

        private void Delete(string path)
        {
            try
            {
                System.IO.Directory.Delete(path, recursive: true);
            }
            catch
            {

            }

            try
            {
                File.Delete(path);
            }
            catch
            {

            }
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
                    package.BuildTime,
                    package.ConstructionTime,
                    package.PublicationTime,
                    package.CreationTime,
                    package.ReadyTime,
                    blazorSupported: _blazorEnabled
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
