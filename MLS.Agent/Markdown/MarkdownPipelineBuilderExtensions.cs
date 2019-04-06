using Markdig;
using Microsoft.DotNet.Try.Markdown;
using MLS.Agent.CommandLine;
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
            var extensions = pipeline.Extensions;

            if (!extensions.Contains<AnnotatedCodeBlockExtension>())
            {
                extensions.Add(
                    new AnnotatedCodeBlockExtension(
                        directoryAccessor,
                        packageRegistry,
                        defaultAnnotations ?? new StartupOptions()));
            }

            return pipeline;
        }
    }
}