using FluentAssertions;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Text;
using System.Threading.Tasks;
using WorkspaceServer;
using WorkspaceServer.Packaging;
using WorkspaceServer.Tests;
using Xunit;
using MLS.Agent.Tools;
using System.IO;
using MLS.Agent.Tests.TestUtility;
using MLS.Agent.CommandLine;
using MLS.Agent.Tools.Extensions;

namespace MLS.Agent.Tests
{
    public class PackagingTests
    {
        [Fact]
        public async Task Pack_project_works()
        {
            var asset = TestAssets.SampleConsole;

            foreach (var file in asset.GetFiles("*.nupkg"))
            {
                file.Delete();
            }

            var console = new TestConsole();
            await PackCommand.Do(new PackOptions(asset), console);
            asset.GetFiles()
                .Should().Contain(f => f.Name.Contains("nupkg"));
        }

        [Fact]
        public async Task Pack_blazor_works_project_works()
        {
            var asset = Create.EmptyWorkspace();
            var dotnet = new Dotnet(asset.Directory);

            // Install blazor templates
            var result = await dotnet.New("-i Microsoft.AspNetCore.Blazor.Templates");
            result.ThrowOnFailure();

            result = await dotnet.New("blazor  -n MLS.Blazor");
            result.ThrowOnFailure();

            var root = Path.Combine(asset.Directory.FullName, "MLS.Blazor");
            dotnet = new Dotnet(new DirectoryInfo(root));

            var toDelete = new[] { "Pages", "Shared", "wwwroot", "_ViewImports.cshtml" };
            foreach (var thing in toDelete)
            {
                var path = Path.Combine(root, thing);
                path.Delete();
            }


            var wwwRootFiles = new [] { "index.html", "interop.js" };
            var pagesFiles = new[] { "Index.cshtml", "Index.cshtml.cs" };
            var rootFiles = new[] { "Program.cs", "Startup.cs" };

            WriteAll(wwwRootFiles, "wwwroot", root);
            WriteAll(pagesFiles, "Pages", root);
            WriteAll(rootFiles, "", root);

            result = await dotnet.AddPackage("MLS.WasmCodeRunner", "1.0.7880001-alpha-c895bf25");
            result.ThrowOnFailure();

            result = await dotnet.AddPackage("NodaTime");
            result = await dotnet.AddPackage("NodaTime.Testing");
            result.ThrowOnFailure();

            result = await dotnet.Publish("");
            result.ThrowOnFailure();
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
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, resourceName);
            File.WriteAllText(path, text);
        }

        private static string ReadManifestResource(string resourceName)
        {
            var assembly = typeof(PackagingTests).Assembly;
            var resoures = assembly.GetManifestResourceNames();
            using (var reader = new StreamReader(assembly.GetManifestResourceStream($"MLS.Agent.Tests.{resourceName}")))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
