namespace Microsoft.DotNet.Try.Markdown
{
    public sealed class SuccessfulCodeFenceOptionParseResult : CodeFenceOptionsParseResult
    {
        public SuccessfulCodeFenceOptionParseResult(CodeLinkBlockOptions options)
        {
            Options = options;
        }

        public CodeLinkBlockOptions Options { get; }
    }
}