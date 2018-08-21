using Microsoft.CodeAnalysis.Text;

namespace WorkspaceServer.Servers.Roslyn.Instrumentation
{
    public static class LinePositionSpanExtensions
    {
        public static bool ContainsLine(this LinePositionSpan viewportSpan, int line) =>
            line < viewportSpan.End.Line && line > viewportSpan.Start.Line;
    }
}
