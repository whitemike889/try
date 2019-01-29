using Markdig;
using WorkspaceServer;

namespace MLS.Agent.Markdown
{
    public static class MarkdownPipelineBuilderExtensions
    {
        public static MarkdownPipelineBuilder UseCodeLinks(this MarkdownPipelineBuilder pipeline, IDirectoryAccessor config, PackageRegistry packageRegistry)
        {
            var extensions = pipeline.Extensions;

            if (!extensions.Contains<CodeLinkExtension>())
            {
                extensions.Add(new CodeLinkExtension(config, packageRegistry));
            }

            return pipeline;
        }
    }
}
