namespace Microsoft.DotNet.Try.Markdown
{
    internal class EditablePreHtmlStyle : HtmlStyleAttribute
    {
        private readonly string _height;

        public EditablePreHtmlStyle(string height)
        {
            _height = height;
        }

        protected override string StyleAttributeString()
        {
            return $@"style=""border:none; height:{_height}""";
        }
    }
}