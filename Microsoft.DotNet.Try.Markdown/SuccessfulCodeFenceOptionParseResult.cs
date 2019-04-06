namespace Microsoft.DotNet.Try.Markdown
{
    public sealed class SuccessfulCodeFenceOptionParseResult : CodeFenceOptionsParseResult
    {
        public SuccessfulCodeFenceOptionParseResult(CodeBlockAnnotations annotations)
        {
            Annotations = annotations;
        }

        public CodeBlockAnnotations Annotations { get; }
    }
}