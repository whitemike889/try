using Microsoft.AspNetCore.Html;

namespace Microsoft.DotNet.Try.Markdown
{
    internal class EditablePreHtmlStyle : HtmlStyleAttribute
    {
        private readonly string _height;

        public EditablePreHtmlStyle(string height)
        {
            _height = height;
        }

        protected override IHtmlContent StyleAttributeString()
        {
            return new HtmlString(  $@"style=""border:none; height:{_height}""");
        }
    }
}