using FluentAssertions;
using WorkspaceServer.Models.Execution;
using Xunit;

namespace WorkspaceServer.Tests
{
    public class RunResultExpectationTests
    {
        [Fact]
        public void RunResultHasOutput()
        {
            nameof(RunResult.Output).Should().Be("Output");
        }

        [Fact]
        public void RunResultHasSucceeded()
        {
            nameof(RunResult.Succeeded).Should().Be("Succeeded");
        }

        [Fact]
        public void RunResultHasException()
        {
            nameof(RunResult.Exception).Should().Be("Exception");
        }

        [Fact]
        public void RunResultHasReturnValue()
        {
            nameof(RunResult.ReturnValue).Should().Be("ReturnValue");
        }
    }
}
