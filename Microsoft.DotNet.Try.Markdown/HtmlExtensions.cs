using System.Web;
using Microsoft.AspNetCore.Html;

namespace Microsoft.DotNet.Try.Markdown
{
    internal static class HtmlExtensions
    {
        public static IHtmlContent HtmlEncode(this string content)
        {
            return new HtmlString(HttpUtility.HtmlEncode(content));
        }

        public static IHtmlContent HtmlAttributeEncode(this string content)
        {
            return new HtmlString(HttpUtility.HtmlAttributeEncode(content));
        }

        public static IHtmlContent ToHtmlContent(this string value)
        {
            return new HtmlString(value);
        }
    }
}