namespace Microsoft.DotNet.Try.Markdown
{
    internal abstract class HtmlStyleAttribute
    {
        public override string ToString()
        {
            return StyleAttributeString();
        }

        protected abstract string StyleAttributeString();
    }
}