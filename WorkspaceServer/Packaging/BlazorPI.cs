using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Clockwise;
using MLS.Agent.Tools;

namespace WorkspaceServer.Packaging
{
    public partial class PackageBuilder
    {
        class BlazorPI : PackageInitializer
        {
            private readonly List<Func<Task>> _addPackages;

            public string EP { get; private set; }

            public BlazorPI(List<Func<Task>> addPackages) : 
                base("blazor", "MLS.Blazor")
            {
                _addPackages = addPackages;
            }

            public override async Task Initialize(DirectoryInfo directory, Budget budget = null)
            {
                //directory = directory.CreateSubdirectory("MLS.Blazor");
                await base.Initialize(directory, budget);
                EP = await MakeBlazorProject(directory, budget);
            }

            private async Task<string> MakeBlazorProject(DirectoryInfo directory, Budget budget)
            {
                //directory = directory.CreateSubdirectory("MLS.Blazor");
                //var initializer = new PackageInitializer("blazor", "MLS.Blazor");
                //await initializer.Initialize(directory, budget);

                var dotnet = new Dotnet(directory);
                var root = directory.FullName;

                var csproj = Path.Combine(root, "MLS.Blazor.csproj");
                var text = File.ReadAllText(csproj);
                File.WriteAllText(csproj, UpdateCSProj(text));

                var toDelete = new[] { "Pages", "Shared", "wwwroot", "_ViewImports.cshtml" };
                foreach (var thing in toDelete)
                {
                    var path = Path.Combine(root, thing);
                    Delete(path);
                }

                var wwwRootFiles = new[] { "index.html", "interop.js" };
                var pagesFiles = new[] { "Index.cshtml", "Index.cshtml.cs" };
                var rootFiles = new[] { "Program.cs", "Startup.cs", "Linker.xml" };

                WriteAll(wwwRootFiles, "wwwroot", root);
                WriteAll(pagesFiles, "Pages", root);
                WriteAll(rootFiles, "", root);

                Modify(root, "wwwroot\\index.html", "/LocalCodeRunner/blazor-console", "/LocalCodeRunner/blazor-nodatime.api");

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

            private void Modify(string root, string file, string toReplace, string replacement)
            {
                file = Path.Combine(root, file);
                var text = File.ReadAllText(file);
                var updated = text.Replace(toReplace, replacement);
                File.WriteAllText(file, updated);
            }

            private string UpdateCSProj(string text)
            {
                var attribute = @"</PropertyGroup>
<PropertyGroup>
    <RootNamespace>MLS.Blazor</RootNamespace>
</PropertyGroup>";
                var u1 = text.Replace("</PropertyGroup>", attribute);

                return u1.Replace("</ItemGroup>", @"
</ItemGroup>
<ItemGroup>
  <BlazorLinkerDescriptor Include=""Linker.xml"" />
</ItemGroup> ");
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
        }
    }
}
