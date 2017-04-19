namespace LanguageServer
{
    internal static class CodeExtensions
    {
        public static bool IsFragment(this string code) =>
            !code.Contains("public static void Main(");
    }
}