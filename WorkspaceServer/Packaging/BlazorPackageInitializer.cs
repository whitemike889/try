using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Clockwise;
using MLS.Agent.Tools;
using MLS.Agent.Tools.Extensions;

namespace WorkspaceServer.Packaging
{
    public class BlazorPackageInitializer : PackageInitializer
    {
        private readonly string _name;
        private readonly List<Func<Task>> _addPackages;

        public FileInfo EntrypointPath { get; private set; }

        public BlazorPackageInitializer(string name, List<Func<Task>> addPackages) :
            base("blazor", "MLS.Blazor")
        {
            this._name = name;
            _addPackages = addPackages;
        }

        public override async Task Initialize(DirectoryInfo directory, Budget budget = null)
        {
            if (directory.Name != "MLS.Blazor")
            {
                throw new ArgumentException(nameof(directory));
            }

            await base.Initialize(directory, budget);
            EntrypointPath = await MakeBlazorProject(directory, budget);
        }

        private async Task<FileInfo> MakeBlazorProject(DirectoryInfo directory, Budget budget)
        {
            var dotnet = new Dotnet(directory);
            var root = directory.FullName;

            AddRootNamespaceAndBlazorLinkerDirective();
            DeleteUnusedFilesFromTemplate(root);
            AddEmbeddedResourceContentToProject(root);
            UpdateFileText(root, "wwwroot\\index.html", "/LocalCodeRunner/blazor-console", $"/LocalCodeRunner/{_name}");

            var result = await dotnet.AddPackage("MLS.WasmCodeRunner", "1.0.7880001-alpha-c895bf25");
            result.ThrowOnFailure();

            foreach (var addPackage in _addPackages)
            {
                await addPackage();
            }

            result = await dotnet.Build("");
            result.ThrowOnFailure();

            return new FileInfo(Path.Combine(directory.FullName,
                "bin\\Debug\\netstandard2.0\\MLS.Blazor.dll"));

            void AddRootNamespaceAndBlazorLinkerDirective()
            {
                UpdateFileText(root, "MLS.Blazor.csproj", "</PropertyGroup>",
                    @"</PropertyGroup>
<PropertyGroup>
    <RootNamespace>MLS.Blazor</RootNamespace>
</PropertyGroup>
<ItemGroup>
  <BlazorLinkerDescriptor Include=""Linker.xml"" />
</ItemGroup>");
            }
        }

        private void AddEmbeddedResourceContentToProject(string root)
        {
            var wwwRootFiles = new[] { "index.html", "interop.js" };
            var pagesFiles = new[] { "Index.cshtml", "Index.cshtml.cs" };
            var rootFiles = new[] { "Program.cs", "Startup.cs", "Linker.xml" };

            WriteResourcesToLocation(wwwRootFiles, Path.Combine(root, "wwwroot"));
            WriteResourcesToLocation(pagesFiles, Path.Combine(root, "Pages"));
            WriteResourcesToLocation(rootFiles, root);
        }

        private static void DeleteUnusedFilesFromTemplate(string root)
        {
            var filesAndDirectoriestoDelete = new[] { "Pages", "Shared", "wwwroot", "_ViewImports.cshtml" };
            foreach (var fOrD in filesAndDirectoriestoDelete)
            {
                Path.Combine(root, fOrD).Delete();
            }
        }

        private void UpdateFileText(string root, string file, string toReplace, string replacement)
        {
            file = Path.Combine(root, file);
            var text = File.ReadAllText(file);
            var updated = text.Replace(toReplace, replacement);
            File.WriteAllText(file, updated);
        }

        private void WriteResourcesToLocation(string[] resources, string targetDirectory)
        {
            foreach (var resource in resources)
            {
                WriteResource(resource, targetDirectory);
            }
        }

        private void WriteResource(string resourceName, string targetDirectory)
        {
            var text = this.GetType().ReadManifestResource(resourceName);
            System.IO.Directory.CreateDirectory(targetDirectory);
            var path = Path.Combine(targetDirectory, resourceName);
            File.WriteAllText(path, text);
        }
    }
}
