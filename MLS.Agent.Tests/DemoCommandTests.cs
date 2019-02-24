using System;
using System.CommandLine;
using System.IO;
using FluentAssertions;
using System.Threading.Tasks;
using MLS.Agent.CommandLine;
using MLS.Agent.Markdown;
using WorkspaceServer;
using WorkspaceServer.Tests;
using Xunit;
using Xunit.Abstractions;

namespace MLS.Agent.Tests
{
    public class DemoCommandTests
    {
        private ITestOutputHelper _output;

        public DemoCommandTests(ITestOutputHelper output)
        {
            _output = output;
        }

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

            _output.WriteLine(console.Out.ToString());
            _output.WriteLine(console.Error.ToString());

            resultCode.Should().Be(0);
        }

        [Fact]
        public async Task Demo_creates_the_output_directory_if_it_does_not_exist()
        {
            var console = new TestConsole();

            var outputDirectory = new DirectoryInfo(
                Path.Combine(
                    Create.EmptyWorkspace().Directory.FullName,
                    Guid.NewGuid().ToString("N")));

            await DemoCommand.Do(new DemoOptions(outputDirectory), console);

            outputDirectory.Refresh();

            outputDirectory.Exists.Should().BeTrue();
        }

        [Fact]
        public async Task Demo_returns_an_error_if_the_output_directory_is_not_empty()
        { var console = new TestConsole();

            var outputDirectory = Create.EmptyWorkspace().Directory;

            File.WriteAllText(Path.Combine(outputDirectory.FullName, "a file.txt"), "");

            await DemoCommand.Do(new DemoOptions(outputDirectory), console);

            var resultCode = await VerifyCommand.Do(
                                 new VerifyOptions(outputDirectory),
                                 console,
                                 () => new FileSystemDirectoryAccessor(outputDirectory),
                                 new PackageRegistry());

            resultCode.Should().NotBe(0);
        }
    }
}