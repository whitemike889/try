using Markdig;
using Markdig.Renderers;

namespace MLS.Agent.Markdown
{
    public class CodeLinkExtension : IMarkdownExtension
    {
        private readonly IDirectoryAccessor _directoryAccessor;

        public CodeLinkExtension(IDirectoryAccessor directoryAccessor)
        {
            _directoryAccessor = directoryAccessor;
        }

        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            if (!pipeline.BlockParsers.Contains<CodeLinkBlockParser>())
            {
                // It should execute before the FencedCodeBlockParser
                pipeline.BlockParsers.Insert(0, new CodeLinkBlockParser(_directoryAccessor));
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
