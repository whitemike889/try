using Microsoft.DotNet.Try.Markdown;
using MLS.Protocol.Execution;

namespace MLS.Agent.Markdown
{
    public static class CodeLinkBlockExtensions
    {
        public static Workspace.Buffer GetBufferAsync(
            this CodeLinkBlock block,
            IDirectoryAccessor directoryAccessor,
            MarkdownFile markdownFile)
        {
            if (block.Options is LocalCodeLinkBlockOptions localOptions)
            {
                var absolutePath = directoryAccessor.GetFullyQualifiedPath(localOptions.SourceFile).FullName;
                var bufferId = new BufferId(absolutePath, block.Options.Region);
                return new Workspace.Buffer(bufferId, block.SourceCode);
            }
            else
            {
                return null;
            }
        }

        public static string ProjectOrPackageName(this CodeLinkBlock block)
        {
            return
                (block.Options as LocalCodeLinkBlockOptions)?.Project?.FullName ??
                block.Options?.Package;
        }
    }
}