namespace Microsoft.DotNet.Try.Markdown
{
    internal class HiddenPreHtmlStyle : HtmlStyleAttribute
    {
        protected override string StyleAttributeString()
        {
            return @"style=""border:none; margin:0px; padding:0px; visibility:hidden; display: none;""";
        }
    }
}