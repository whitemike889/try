using System.Collections.Generic;
using System.Linq;
using FluentAssertions;

namespace WorkspaceServer.Tests
{
    public static class StringAssertionExtensions
    {
        public static void ShouldMatch(this IReadOnlyCollection<string> actual, params string[] expected)
        {
            for (var i = 0; i < expected.Length; i++)
            {
                actual.ElementAt(i).Should().Match(expected[i]);
            }
        }

    }
}
