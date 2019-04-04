using Microsoft.DotNet.Try.Markdown;

namespace MLS.Agent.Markdown
{
    public static class CodeLinkBlockExtensions
    {
        public static string ProjectOrPackageName(this CodeLinkBlock block)
        {
            return
                (block.Options as LocalCodeLinkBlockOptions)?.Project?.FullName ??
                block.Options?.Package;
        }
    }
}