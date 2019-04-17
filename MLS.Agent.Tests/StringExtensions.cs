using Microsoft.DotNet.Try.Protocol.Tests;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MLS.Agent.Tests
{
    internal static class StringExtensions
    {
        public static string FormatJson(this string value)
        {
            var s = JToken.Parse(value).ToString(Formatting.Indented);
            return s.EnforceLF();
        }
    }
}