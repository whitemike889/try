namespace MLS.Agent.Markdown
{
    public static class CodeLinkBlockExtensions
    {
        public static string ProjectOrPackageName(this CodeLinkBlock block)
        {
            return block.ProjectFile?.FullName ?? block.Package;
        }
    }
}