using System;

namespace WorkspaceServer.Tests
{
    public class CodeManipulation
    {
        public static string EnforceLF(string source)
        {
            return source?.Replace(Environment.NewLine, "\n");
        }

        public static (string processed, int markLocation) ProcessMarkup(string source)
        {
            var normalised = EnforceLF(source);
            var markLocation = normalised.IndexOf("$$");
            var processed = normalised.Replace("$$", string.Empty);
            return (processed, markLocation);
        }
    }
}