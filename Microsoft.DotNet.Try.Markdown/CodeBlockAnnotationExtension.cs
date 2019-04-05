using Markdig;
using Markdig.Renderers;

namespace Microsoft.DotNet.Try.Markdown
{
    public class CodeBlockAnnotationExtension : IMarkdownExtension
    {
        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            if (!pipeline.BlockParsers.Contains<CodeLinkBlockParser>())
            {
                var optionsParser = new CodeFenceOptionsParser();

                // It should execute before Markdig's default FencedCodeBlockParser
                pipeline.BlockParsers.Insert(
                    index: 0,
                    new CodeLinkBlockParser(optionsParser));
            }
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            var htmlRenderer = renderer as HtmlRenderer;
            var renderers = htmlRenderer?.ObjectRenderers;
            if (renderers != null && !renderers.Contains<CodeLinkBlockRenderer>())
            {
                var codeLinkBlockRenderer = new CodeLinkBlockRenderer();
                renderers.Insert(0, codeLinkBlockRenderer);
            }
        }
    }
}