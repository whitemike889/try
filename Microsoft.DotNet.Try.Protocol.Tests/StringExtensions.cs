namespace Microsoft.DotNet.Try.Protocol.Tests
{
    public static class StringExtensions
    {
        public static string EnforceLF(this string source)
        {
            return source?.Replace("\r\n", "\n") ?? string.Empty;
        }
    }
}