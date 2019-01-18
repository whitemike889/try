using FluentAssertions;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Text;
using System.Threading.Tasks;
using Xunit;

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

    }
}
