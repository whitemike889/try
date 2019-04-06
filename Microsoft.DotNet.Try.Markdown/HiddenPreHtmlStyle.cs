using Microsoft.AspNetCore.Html;

namespace Microsoft.DotNet.Try.Markdown
{
    internal class HiddenPreHtmlStyle : HtmlStyleAttribute
    {
        protected override IHtmlContent StyleAttributeString()
        {
            return new HtmlString( @"style=""border:none; margin:0px; padding:0px; visibility:hidden; display: none;""");
        }
    }
}