using Markdig.Renderers;

namespace Microsoft.DotNet.Try.Markdown
{
    internal static class TextRendererBaseExtensions
    {
        public static T WriteLineIf<T>(this T textRendererBase, bool @if, string value)
            where T : HtmlRenderer
        {
            if (@if)
            {
                textRendererBase.WriteLine(value);
            }

            return textRendererBase;
        }
    }
}