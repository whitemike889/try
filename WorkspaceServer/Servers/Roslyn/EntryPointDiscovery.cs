using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace WorkspaceServer.Servers.Roslyn
{
    internal static class SymbolExtensions{
        public static string FullyQualifiedName(this ISymbol symbol)
        {
            var elements = new Stack<string>();
            var cursor = symbol;
            while (cursor != null)
            {
                elements.Push(cursor.MetadataName);
                cursor = cursor.ContainingType ?? (ISymbol) cursor.ContainingNamespace;

                if (cursor == null)
                {
                    break;
                }
            }

            return string.Join(".", elements.Where(name => !string.IsNullOrWhiteSpace(name)));
        }
    }

    internal class EntryPointDiscovery
    {
        private readonly Compilation _resultCompilation;

        public EntryPointDiscovery(Compilation resultCompilation)
        {
            _resultCompilation = resultCompilation;
        }

        public IMethodSymbol ResolveEntryPoint()
        {
            var defaultEntryPoint = _resultCompilation.GetEntryPoint(CancellationToken.None);
            if (defaultEntryPoint == null)
            {
                var visitor = new GetEntryPointVisitor();
                visitor.Visit(_resultCompilation.Assembly.GlobalNamespace);
                defaultEntryPoint = visitor.EntryPointSymbol;
            }

            return defaultEntryPoint;
        }

        public class GetEntryPointVisitor : SymbolVisitor
        {
            private bool _found;
            public IMethodSymbol EntryPointSymbol { get; private set; }

            public override void VisitNamespace(INamespaceSymbol symbol)
            {
                foreach (var childSymbol in symbol.GetMembers())
                {
                    childSymbol.Accept(this);
                    if (_found)
                    {
                        break;
                    }
                }
            }

            public override void VisitNamedType(INamedTypeSymbol symbol)
            {
                foreach (var childSymbol in symbol.GetMembers())
                {
                    childSymbol.Accept(this);
                    if (_found)
                    {
                        break;
                    }
                }
            }

            public override void VisitMethod(IMethodSymbol symbol)
            {
                if (symbol.IsStatic && symbol.Name == "Main")
                {
                    EntryPointSymbol =  symbol;
                    _found = true;

                }
            }
        }
    }
}