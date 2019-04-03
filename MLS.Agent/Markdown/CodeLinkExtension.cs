using System;
using Markdig;
using Markdig.Renderers;
using Microsoft.DotNet.Try.Markdown;
using WorkspaceServer;

namespace MLS.Agent.Markdown
{
    public class CodeLinkExtension : IMarkdownExtension
    {
        private readonly IDirectoryAccessor _directoryAccessor;
        private readonly PackageRegistry _packageRegistry;
        private readonly IDefaultCodeLinkBlockOptions _defaultOptions;

        public CodeLinkExtension(
            IDirectoryAccessor directoryAccessor, 
            PackageRegistry packageRegistry,
            IDefaultCodeLinkBlockOptions defaultOptions = null)
        {
            _directoryAccessor = directoryAccessor ?? throw new ArgumentNullException(nameof(directoryAccessor));
            _packageRegistry = packageRegistry ?? throw new ArgumentNullException(nameof(packageRegistry));
            _defaultOptions = defaultOptions;
        }

        public bool InlineControls { get; set; }

        public bool EnablePreviewFeatures { get; set; }

        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            if (!pipeline.BlockParsers.Contains<CodeLinkBlockParser>())
            {
                var optionsParser = new LocalCodeFenceOptionsParser(
                    _directoryAccessor, 
                    _packageRegistry,
                    _defaultOptions);

                // It should execute before the FencedCodeBlockParser
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
                var codeLinkBlockRenderer = new CodeLinkBlockRenderer
                {
                    InlineControls = InlineControls,
                    EnablePreviewFeatures = EnablePreviewFeatures
                };
                renderers.Insert(0, codeLinkBlockRenderer);
            }
        }
    }
}
