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

            result = await dotnet.New("blazor");
            result.ThrowOnFailure();

            var toDelete = new[] { "Pages", "Shared", "wwwroot", "_ViewImports.cshtml" };
            foreach (var thing in toDelete)
            {
                var path = Path.Combine(asset.Directory.FullName, thing);
                Delete(path);
            }

            result = await dotnet.AddPackage("MLS.BlazorSource", "0.1.0");
            result.ThrowOnFailure();

            result = await dotnet.AddPackage("MLS.WasmCodeRunner", "1.0.0--00000001.1");
            result.ThrowOnFailure();

            result = await dotnet.AddPackage("NodaTime", "2.3.0");
            result.ThrowOnFailure();

            result = await dotnet.Publish("");
            result.ThrowOnFailure();
        }

        private void Delete(string path)
        {
            try
            {
                Directory.Delete(path, recursive: true);
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
