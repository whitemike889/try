using System.Web;
using Microsoft.AspNetCore.Html;

namespace MLS.Agent
{
    public static class StringExtensions
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