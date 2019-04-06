using Microsoft.DotNet.Try.Markdown;
using MLS.Protocol.Execution;

namespace MLS.Agent.Markdown
{
    public static class AnnotatedCodeBlockExtensions
    {
        public static Workspace.Buffer GetBufferAsync(
            this AnnotatedCodeBlock block,
            IDirectoryAccessor directoryAccessor,
            MarkdownFile markdownFile)
        {
            if (block.Annotations is LocalCodeBlockAnnotations localOptions)
            {
                var absolutePath = directoryAccessor.GetFullyQualifiedPath(localOptions.SourceFile).FullName;
                var bufferId = new BufferId(absolutePath, block.Annotations.Region);
                return new Workspace.Buffer(bufferId, block.SourceCode);
            }
            else
            {
                return null;
            }
        }

        public static string ProjectOrPackageName(this AnnotatedCodeBlock block)
        {
            return
                (block.Annotations as LocalCodeBlockAnnotations)?.Project?.FullName ??
                block.Annotations?.Package;
        }
    }
}