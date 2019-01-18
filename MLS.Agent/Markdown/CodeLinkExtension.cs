using Markdig;
using Markdig.Renderers;

namespace MLS.Agent.Markdown
{
    public class CodeLinkExtension : IMarkdownExtension
    {
        private readonly Configuration _config;

        public CodeLinkExtension(Configuration config)
        {
            _config = config;
        }

        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            if (!pipeline.BlockParsers.Contains<CodeLinkBlockParser>())
            {
                // It should execute before the FencedCodeBlockParser
                pipeline.BlockParsers.Insert(0, new CodeLinkBlockParser(_config));
            }
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            var htmlRenderer = renderer as HtmlRenderer;
            var renderers = htmlRenderer?.ObjectRenderers;
            if (renderers != null && !renderers.Contains<CodeLinkBlockRenderer>())
            {
                renderers.Insert(0, new CodeLinkBlockRenderer());
            }
        }
    }
}
