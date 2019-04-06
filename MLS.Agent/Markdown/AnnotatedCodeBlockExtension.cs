using System;
using Markdig;
using Markdig.Renderers;
using Microsoft.DotNet.Try.Markdown;
using WorkspaceServer;

namespace MLS.Agent.Markdown
{
    public class AnnotatedCodeBlockExtension : IMarkdownExtension
    {
        private readonly IDirectoryAccessor _directoryAccessor;
        private readonly PackageRegistry _packageRegistry;
        private readonly IDefaultCodeBlockAnnotations _defaultAnnotations;

        public AnnotatedCodeBlockExtension(
            IDirectoryAccessor directoryAccessor, 
            PackageRegistry packageRegistry,
            IDefaultCodeBlockAnnotations defaultAnnotations = null)
        {
            _directoryAccessor = directoryAccessor ?? throw new ArgumentNullException(nameof(directoryAccessor));
            _packageRegistry = packageRegistry ?? throw new ArgumentNullException(nameof(packageRegistry));
            this._defaultAnnotations = defaultAnnotations;
        }

        public bool InlineControls { get; set; }

        public bool EnablePreviewFeatures { get; set; }

        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            // FIX: dedupe
            if (!pipeline.BlockParsers.Contains<AnnotatedCodeBlockParser>())
            {
                var optionsParser = new LocalCodeFenceAnnotationsParser(
                    _directoryAccessor, 
                    _packageRegistry,
                    _defaultAnnotations);

                // It should execute before the FencedCodeBlockParser
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
                var codeLinkBlockRenderer = new AnnotatedCodeBlockRenderer
                {
                    InlineControls = InlineControls,
                    EnablePreviewFeatures = EnablePreviewFeatures
                };
                renderers.Insert(0, codeLinkBlockRenderer);
            }
        }
    }
}
