using Markdig;
using Microsoft.DotNet.Try.Markdown;
using WorkspaceServer;

namespace MLS.Agent.Markdown
{
    public static class MarkdownPipelineBuilderExtensions
    {
        public static MarkdownPipelineBuilder UseCodeBlockAnnotations(
            this MarkdownPipelineBuilder pipeline,
            IDirectoryAccessor directoryAccessor,
            PackageRegistry packageRegistry,
            IDefaultCodeBlockAnnotations defaultAnnotations = null)
        {
            return pipeline.UseCodeBlockAnnotations(
                new LocalCodeFenceAnnotationsParser(
                    directoryAccessor,
                    packageRegistry,
                    defaultAnnotations));
        }
    }
}