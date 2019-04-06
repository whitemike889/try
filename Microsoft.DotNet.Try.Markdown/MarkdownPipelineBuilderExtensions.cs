using Markdig;

namespace Microsoft.DotNet.Try.Markdown
{
    public static class MarkdownPipelineBuilderExtensions
    {
        public static MarkdownPipelineBuilder UseCodeBlockAnnotations(
            this MarkdownPipelineBuilder pipeline, 
            CodeFenceAnnotationsParser annotationsParser = null)
        {
            var extensions = pipeline.Extensions;

            if (!extensions.Contains<CodeBlockAnnotationExtension>())
            {
                extensions.Add(new CodeBlockAnnotationExtension(annotationsParser));
            }

            return pipeline;
        }
    }
}