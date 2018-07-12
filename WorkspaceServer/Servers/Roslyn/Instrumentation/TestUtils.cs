using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WorkspaceServer.Servers.Roslyn.Instrumentation
{
    public static class TestUtils
    {
        public static string RemoveWhitespace(string input) => Regex.Replace(input, @"\s", "");
    }
}

