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
            await PackageCommand.Do(asset, console);
            asset.GetFiles()
                .Should().Contain(f => f.Name.Contains("nupkg"));
        }

        [Fact]
        public async Task Pack_blazor_works_project_works()
        {
            //var asset = Create.EmptyWorkspace();
            //var dotnet = new Dotnet(asset.Directory);

            //// Install blazor templates
            //var result = await dotnet.New("", "-i Microsoft.AspNetCore.Blazor.Templates");
            //result.ThrowOnFailure();

            //result = await dotnet.New("blazor");


            //foreach (var file in asset.GetFiles("*.nupkg"))
            //{
            //    file.Delete();
            //}

            //var console = new TestConsole();
            //await PackageCommand.Do(asset, console);
            //asset.GetFiles()
            //    .Should().Contain(f => f.Name.Contains("nupkg"));
        }

    }
}
