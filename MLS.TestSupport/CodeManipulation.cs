using System;

namespace MLS.TestSupport
{
    public static  class CodeManipulation
    {
        public static string EnforceLF(string source)
        {
            return source?.Replace(Environment.NewLine, "\n")?? string.Empty;
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