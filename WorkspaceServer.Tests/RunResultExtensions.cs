using System;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using WorkspaceServer.Models.Execution;

namespace WorkspaceServer.Tests
{
    public static class RunResultExtensions
    {
        public static void ShouldFailWithOutput(this RunResult result, params string[] output)
        {
            using (new AssertionScope("result"))
            {
                result.Succeeded.Should().BeFalse();
                result.Output.ShouldBeEquivalentTo(output);
                result.Exception.Should().BeNull();
            }
        }

        public static void ShouldSucceedWithOutput(this RunResult result, params string[] output)
        {
            using (new AssertionScope("result"))
            {
                result.Succeeded.Should().BeTrue();
                for (var i = 0; i < output.Length; i++)
                {
                    var line = output[i];
                    line.Should().Match(result.Output.ElementAt(i));
                }

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
