// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
                    _command = "prepare-package";
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
        public async Task Parse_extract_calls_prepare()
        {
            await _parser.InvokeAsync("prepare-package", _console);
            _command.Should().Be("prepare-package");
        }
    }
}