using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using FluentAssertions;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace MLS.PackageTool.Tests
{
    public class CommandLineParserTests
    {
        private readonly ITestOutputHelper _output;
        private readonly TestConsole _console = new TestConsole();
        private readonly Parser _parser;
        private string _command;

        public CommandLineParserTests(ITestOutputHelper output)
        {
            _output = output;
            _parser = CommandLineParser.Create(
                getAssembly: (_) => { _command = "getAssembly"; },
                extract: (_) => {
                    _command = "extract-package";
                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task Parse_locate_assembly_locates_assembly()
        {
            await _parser.InvokeAsync("locate-projects", _console);
            _command.Should().Be("getAssembly");
        }

        [Fact]
        public async Task Parse_extract_calls_extract()
        {
            await _parser.InvokeAsync("extract-package", _console);
            _command.Should().Be("extract-package");
        }
    }
}