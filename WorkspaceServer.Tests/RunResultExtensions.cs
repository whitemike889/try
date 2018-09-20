using System;
using FluentAssertions;
using FluentAssertions.Execution;
using MLS.Protocol.Execution;

namespace WorkspaceServer.Tests
{
    public static class RunResultExtensions
    {
        public static void ShouldFailWithOutput(this RunResult result, params string[] expected)
        {
            using (new AssertionScope("result"))
            {
                result.Succeeded.Should().BeFalse();
                result.Output.ShouldMatch(expected);
                result.Exception.Should().BeNull();
            }
        }

        public static void ShouldSucceedWithOutput(this RunResult result, params string[] expected)
        {
            using (new AssertionScope("result"))
            {
                result.Succeeded.Should().BeTrue();
                result.Output.ShouldMatch(expected);
                result.Exception.Should().BeNull();
            }
        }

        public static void ShouldSucceedWithNoOutput(this RunResult result) =>
            result.ShouldSucceedWithOutput(Array.Empty<string>());

        public static void ShouldFailWithExceptionContaining(this RunResult result, string text, params string[] output)
        {
            using (new AssertionScope("result"))
            {
                result.Succeeded.Should().BeFalse();
                result.Output.Should().NotBeNull();
                result.Output.ShouldMatch(output);
                result.Exception.Should().Contain(text);
            }
        }

        public static void ShouldSucceedWithExceptionContaining(this RunResult result, string text, params string[] output)
        {
            using (new AssertionScope("result"))
            {
                result.Succeeded.Should().BeTrue();
                result.Output.ShouldMatch(output);
                result.Exception.Should().Contain(text);
            }
        }
    }
}
