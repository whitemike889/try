using System.IO;
using System.Web;

namespace MLS.Agent
{
    public static class StringExtensions
    {
        public static string HtmlEncode(this string content)
        {
            return HttpUtility.HtmlEncode(content);
        }

        public static string HtmlAttributeEncode(this string content)
        {
            return HttpUtility.HtmlAttributeEncode(content);
        }
    }
}
