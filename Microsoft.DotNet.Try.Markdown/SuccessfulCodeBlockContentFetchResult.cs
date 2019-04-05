namespace Microsoft.DotNet.Try.Markdown
{
    public sealed class SuccessfulCodeBlockContentFetchResult : CodeBlockContentFetchResult
    {
        public SuccessfulCodeBlockContentFetchResult(string content)
        {
            Content = content;
        }

        public string Content { get; }
    }
}