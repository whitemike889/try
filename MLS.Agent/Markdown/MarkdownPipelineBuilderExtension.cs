using Markdig;
using Markdig.Helpers;

namespace MLS.Agent.Markdown
{
    public static class MarkdownPipelineBuilderExtension
    {
        public static MarkdownPipelineBuilder UseCodeLinks(this MarkdownPipelineBuilder pipeline, Configuration config)
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
