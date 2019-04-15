using FluentAssertions;
using MLS.WasmCodeRunner;
using Xunit;

namespace MLS.WasmCodeRunnerTests
{
    public class CommandLineArgumentTests
    {
        [Theory]
        [InlineData("one two", new []{"one", "two"})]
        [InlineData("one two \"third one\" fourth", new[] { "one", "two", "third one", "fourth" })]
        [InlineData("\"C:\\Program Files\"", new[] { "C:\\Program Files"})]
        [InlineData("--region \"region name here\"", new[] { "--region", "region name here" })]
        public void Give_A_single_string_it_is_split_in_arg_list(string input, string[] expected)
        {
            var args = CodeRunner.SplitCommandLine(input);
            args.Should().BeEquivalentTo(expected);
        }
    }
}