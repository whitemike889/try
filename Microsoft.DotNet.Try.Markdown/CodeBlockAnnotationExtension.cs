using Markdig;
using Markdig.Renderers;

namespace Microsoft.DotNet.Try.Markdown
{
    public class CodeBlockAnnotationExtension : IMarkdownExtension
    {
        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            if (!pipeline.BlockParsers.Contains<AnnotatedCodeBlockParser>())
            {
                var optionsParser = new CodeFenceAnnotationsParser();

                // It should execute before Markdig's default FencedCodeBlockParser
                pipeline.BlockParsers.Insert(
                    index: 0,
                    new AnnotatedCodeBlockParser(optionsParser));
            }
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            var htmlRenderer = renderer as HtmlRenderer;
            var renderers = htmlRenderer?.ObjectRenderers;
            if (renderers != null && !renderers.Contains<AnnotatedCodeBlockRenderer>())
            {
                var codeLinkBlockRenderer = new AnnotatedCodeBlockRenderer();
                renderers.Insert(0, codeLinkBlockRenderer);
            }
        }
    }
}