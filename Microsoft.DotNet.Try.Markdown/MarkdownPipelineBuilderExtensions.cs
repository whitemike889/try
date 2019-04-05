using Markdig;

namespace Microsoft.DotNet.Try.Markdown
{
    public static class MarkdownPipelineBuilderExtensions
    {
        public static MarkdownPipelineBuilder UseAnnotatedCodeBlocks(
            this MarkdownPipelineBuilder pipeline)
        {
            var extensions = pipeline.Extensions;

            if (!extensions.Contains<CodeBlockAnnotationExtension>())
            {
                extensions.Add(new CodeBlockAnnotationExtension());
            }

            return pipeline;
        }
    }
}