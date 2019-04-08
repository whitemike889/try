using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.DotNet.Try.Project.Generators
{
    public static class CodeManipulation
    {
        public static string FormatJson(this string value)
        {
            return EnforceLF(JToken.Parse(value).ToString(Formatting.Indented));
        }

        public static string EnforceLF(this string source)
        {
            return source?.Replace("\r\n", "\n") ?? string.Empty;
        }
    }
}