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

        public static string NormalizePath(this string path)
        {
            return Path.GetFullPath(path.Replace('\\', Path.DirectorySeparatorChar));
        }
    }
}
