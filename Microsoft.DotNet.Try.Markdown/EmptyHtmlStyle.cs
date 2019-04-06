using Microsoft.AspNetCore.Html;

namespace Microsoft.DotNet.Try.Markdown
{
    internal class EmptyHtmlStyle : HtmlStyleAttribute
    {
        protected override IHtmlContent StyleAttributeString()
        {
            return HtmlString.Empty;
        }
    }
}