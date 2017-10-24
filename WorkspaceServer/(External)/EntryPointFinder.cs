using System.Linq;
using Microsoft.CodeAnalysis;

namespace WorkspaceServer
{
    internal class EntryPointFinder : AbstractEntryPointFinder
    {
        protected override bool MatchesMainMethodName(string name)
        {
            return name == "Main";
        }

        public static IMethodSymbol FindEntryPoint(INamespaceSymbol symbol)
        {
            var visitor = new EntryPointFinder();
            visitor.Visit(symbol);
            return visitor.EntryPoints.SingleOrDefault();
        }
    }
}
