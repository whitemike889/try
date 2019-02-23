using System;
using MLS.Agent.CommandLine;
using Xunit;
using System.CommandLine;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using WorkspaceServer.Tests;
using FluentAssertions;
using MLS.Agent.Markdown;
using WorkspaceServer;

namespace MLS.Agent.Tests
{
    public class DemoCommandTests
    {
        [Fact]
        public async Task Demo_project_passes_verification()
        {
            var console = new TestConsole();

            var outputDirectory = Create.EmptyWorkspace().Directory;

            await DemoCommand.Do(new DemoOptions(outputDirectory), console);

            var resultCode = await VerifyCommand.Do(
                new VerifyOptions(outputDirectory),
                console,
                () => new FileSystemDirectoryAccessor(outputDirectory),
                new PackageRegistry());

            resultCode.Should().Be(0);
        }
    }
}
