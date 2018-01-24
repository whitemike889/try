using System;
using FluentAssertions;
using System.Linq;
using FluentAssertions.Execution;
using WorkspaceServer.Models.Execution;

namespace WorkspaceServer.Tests
{
    public static class RunResultExtensions
    {
        public static void ShouldFailWithOutput(this RunResult result, params string[] output)
        {
            result.ShouldBeEquivalentTo(new
            {
                Succeeded = false,
                Output = output,
                Exception = (string) null
            }, config => config.ExcludingMissingMembers());
        }

        public static void ShouldSucceedWithOutput(this RunResult result, params string[] output)
        {
            result.ShouldBeEquivalentTo(new
            {
                Succeeded = true,
                Output = output,
                Exception = (string) null
            }, config => config.ExcludingMissingMembers());
        }

        public static void ShouldSucceedWithNoOutput(this RunResult result) =>
            result.ShouldSucceedWithOutput(Array.Empty<string>());

        public static void ShouldFailWithExceptionContaining(this RunResult result, string text, params string[] output)
        {
            using (new AssertionScope("result"))
            {
                result.Succeeded.Should().BeFalse();
                result.Output.Should().NotBeNull();
                result.Output.ShouldBeEquivalentTo(output);
                result.Exception.Should().Contain(text);
            }
        }

        public static void ShouldSucceedWithExceptionContaining(this RunResult result, string text, params string[] output)
        {
            using (new AssertionScope("result"))
            {
                result.Succeeded.Should().BeTrue();
                result.Output.ShouldBeEquivalentTo(output);
                result.Exception.Should().Contain(text);
            }
        }
    }
}
