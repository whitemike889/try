using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WorkspaceServer.Servers.Roslyn.Instrumentation
{
    public class Augmentation : ISerializableOnce
    {
        public Augmentation(CSharpSyntaxNode associatedStatment,
                            IEnumerable<ISymbol> locals,
                            IEnumerable<ISymbol> fields,
                            IEnumerable<ISymbol> parameters,
                            IEnumerable<ISymbol> internalLocals)
        {
            AssociatedStatement = associatedStatment ?? throw new ArgumentNullException(nameof(associatedStatment));
            Locals = locals ?? Array.Empty<ISymbol>();
            Fields = fields ?? Array.Empty<ISymbol>();
            Parameters = parameters ?? Array.Empty<ISymbol>();
            InternalLocals = internalLocals ?? Array.Empty<ISymbol>();
        }

        public CSharpSyntaxNode AssociatedStatement { get; }
        public IEnumerable<ISymbol> Locals { get; }
        public IEnumerable<ISymbol> Fields { get; }
        public IEnumerable<ISymbol> Parameters { get; }
        public IEnumerable<ISymbol> InternalLocals { get; }

        public override bool Equals(object obj)
        {
            var augmentation = obj as Augmentation;
            return augmentation != null &&
                   EqualityComparer<CSharpSyntaxNode>.Default.Equals(AssociatedStatement, augmentation.AssociatedStatement) &&
                   EqualityComparer<IEnumerable<ISymbol>>.Default.Equals(Locals, augmentation.Locals) &&
                   EqualityComparer<IEnumerable<ISymbol>>.Default.Equals(Fields, augmentation.Fields) &&
                   EqualityComparer<IEnumerable<ISymbol>>.Default.Equals(Parameters, augmentation.Parameters) &&
                   EqualityComparer<IEnumerable<ISymbol>>.Default.Equals(InternalLocals, augmentation.InternalLocals);
        }

        public override int GetHashCode()
        {
            var hashCode = 1495038390;
            hashCode = hashCode * -1521134295 + EqualityComparer<CSharpSyntaxNode>.Default.GetHashCode(AssociatedStatement);
            hashCode = hashCode * -1521134295 + EqualityComparer<IEnumerable<ISymbol>>.Default.GetHashCode(Locals);
            hashCode = hashCode * -1521134295 + EqualityComparer<IEnumerable<ISymbol>>.Default.GetHashCode(Fields);
            hashCode = hashCode * -1521134295 + EqualityComparer<IEnumerable<ISymbol>>.Default.GetHashCode(Parameters);
            hashCode = hashCode * -1521134295 + EqualityComparer<IEnumerable<ISymbol>>.Default.GetHashCode(InternalLocals);
            return hashCode;
        }

        public string Serialize()
        {
            var position = this.AssociatedStatement.GetLocation().GetLineSpan();

            var symbolInformation = new[] {
                PrintSymbols("locals", this.Locals),
                PrintSymbols("parameters", this.Parameters),
                PrintSymbols("fields", this.Fields)
            }.Join();

            var str = $@"
    \""filePosition\"": {{
        \""line\"": {position.StartLinePosition.Line},
        \""character\"": {position.StartLinePosition.Character},
        \""file\"": \""{Path.GetFileName(position.Path)}\""
    }},
    \""stackTrace\"": \"" "" + new System.Diagnostics.StackTrace(false).ToString() + "" \"",
    {symbolInformation}
";

            return str;
        }

        private string PrintSymbols(string symbolType, IEnumerable<ISymbol> symbols)
        {
            var serializedSymbols = symbols.Select(symbol =>
            {
                var syntax = symbol.DeclaringSyntaxReferences.First().GetSyntax();
                var location = syntax.Span;
                if (syntax is VariableDeclaratorSyntax vds)
                {
                    location = vds.Identifier.Span;
                }
                else if (syntax is ForEachStatementSyntax fes)
                {
                    location = fes.Identifier.Span;
                }

                var str = $@"
{{
    \""name\"": \""{symbol.Name}\"",
    \""value\"": \""""+({symbol.Name} != null ? System.Uri.EscapeDataString({symbol.Name}.ToString()) : ""null"") + ""\"",
    \""declaredAt\"": {{ \""start\"" : {location.Start}, \""end\"" : {location.End} }}
}}";

                return str;

            }).Join();

            var toJsonArray = $"\\\"{symbolType}\\\" : [{serializedSymbols}]";
            return toJsonArray;
        }


    }
}
