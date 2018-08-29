using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace WorkspaceServer.Tests.Instrumentation
{
    public static class TestUtils
    {
        public static string RemoveWhitespace(string input) => Regex.Replace(input, @"\s", "");
    }
}

