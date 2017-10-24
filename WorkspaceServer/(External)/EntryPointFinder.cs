using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace WorkspaceServer
{
    internal class EntryPointFinder : AbstractEntryPointFinder
    {
        protected override bool MatchesMainMethodName(string name)
        {
            return name == "Main";
        }
 
        public static IEnumerable<INamedTypeSymbol> FindEntryPoints(INamespaceSymbol symbol)
        {
            var visitor = new EntryPointFinder();
            visitor.Visit(symbol);
            return visitor.EntryPoints;
        }
    }
}