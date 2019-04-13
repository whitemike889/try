using Microsoft.CodeAnalysis;
using Microsoft.DotNet.Try.Project.Execution;
using Workspace = Microsoft.DotNet.Try.Protocol.Workspace;

namespace WorkspaceServer.Servers.Roslyn
{
    public static class DocumentExtensions
    {
        public static bool IsMatch(this Document doc, Workspace.File file) => 
            doc.IsMatch(file.Name);

        public static bool IsMatch(this Document d, SourceFile source) => 
            d.IsMatch(source.Name);

        public static bool IsMatch(this Document d, string sourceName) => 
            d.Name == sourceName || d.FilePath == sourceName;
    }
}