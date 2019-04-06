namespace Microsoft.DotNet.Try.Markdown
{
    internal class EmptyHtmlStyle : HtmlStyleAttribute
    {
        protected override string StyleAttributeString()
        {
            return string.Empty;
        }
    }
}