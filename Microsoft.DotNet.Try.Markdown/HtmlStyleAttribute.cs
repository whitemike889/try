using Microsoft.AspNetCore.Html;

namespace Microsoft.DotNet.Try.Markdown
{
    internal abstract class HtmlStyleAttribute
    {
        public override string ToString()
        {
            return StyleAttributeString().ToString();
        }

        protected abstract IHtmlContent StyleAttributeString();
    }
}