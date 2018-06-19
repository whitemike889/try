using Microsoft.CodeAnalysis;

namespace WorkspaceServer.Servers.Roslyn.Instrumentation
{
    public interface ISerializableEveryLine
    {
        string SerializeForLine(SyntaxNode line);
    }
}
