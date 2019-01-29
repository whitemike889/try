using Markdig;

namespace MLS.Agent.Markdown
{
    public static class MarkdownPipelineBuilderExtensions
    {
        public static MarkdownPipelineBuilder UseCodeLinks(this MarkdownPipelineBuilder pipeline, IDirectoryAccessor config)
        {
            var extensions = pipeline.Extensions;

            if (!extensions.Contains<CodeLinkExtension>())
            {
                extensions.Add(new CodeLinkExtension(config));
            }

            return pipeline;
        }
    }
}
