using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WorkspaceServer.Tests
{
    public static class CodeManipulation
    {
        public static string FormatJson(this string value)
        {
            return EnforceLF(JToken.Parse(value).ToString(Formatting.Indented));
        }

        public static string EnforceLF(this string source)
        {
            return source?.Replace(Environment.NewLine, "\n") ?? string.Empty;
        }

        public static (string processed, int markLocation) ProcessMarkup(string source)
        {
            var normalised = EnforceLF(source);
            var markLocation = normalised.IndexOf("$$", StringComparison.InvariantCulture);
            var processed = normalised.Replace("$$", string.Empty);
            return (processed, markLocation);
        }
    }
}
