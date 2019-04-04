using Markdig;
using Microsoft.DotNet.Try.Markdown;
using MLS.Agent.CommandLine;
using WorkspaceServer;

namespace MLS.Agent.Markdown
{
    public static class MarkdownPipelineBuilderExtensions
    {
        public static MarkdownPipelineBuilder UseCodeLinks(
            this MarkdownPipelineBuilder pipeline,
            IDirectoryAccessor directoryAccessor,
            PackageRegistry packageRegistry,
            IDefaultCodeLinkBlockOptions defaultOptions = null)
        {
            var extensions = pipeline.Extensions;

            if (!extensions.Contains<CodeLinkExtension>())
            {
                extensions.Add(
                    new CodeLinkExtension(
                        directoryAccessor,
                        packageRegistry,
                        defaultOptions ?? new StartupOptions()));
            }

            return pipeline;
        }
    }
}