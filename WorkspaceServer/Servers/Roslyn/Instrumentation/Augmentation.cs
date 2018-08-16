using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using WorkspaceServer.Models.Instrumentation;

namespace WorkspaceServer.Servers.Roslyn.Instrumentation
{
    public class Augmentation : ISerializableOnce
    {
        public Augmentation(CSharpSyntaxNode associatedStatment,
                            IEnumerable<ISymbol> locals,
                            IEnumerable<ISymbol> fields,
                            IEnumerable<ISymbol> parameters,
                            IEnumerable<ISymbol> internalLocals,
                            FilePosition position = null)
        {
            AssociatedStatement = associatedStatment ?? throw new ArgumentNullException(nameof(associatedStatment));
            Locals = locals ?? Array.Empty<ISymbol>();
            Fields = fields ?? Array.Empty<ISymbol>();
            Parameters = parameters ?? Array.Empty<ISymbol>();
            InternalLocals = internalLocals ?? Array.Empty<ISymbol>();

            if (position == null)
            {
                var linePosition = AssociatedStatement.GetLocation().GetLineSpan();
                CurrentFilePosition = new FilePosition
                {
                    Line = linePosition.StartLinePosition.Line,
                    Character = linePosition.StartLinePosition.Character,
                    File = Path.GetFileName(linePosition.Path)
                };
            }
            else
            {
                CurrentFilePosition = position;
            }
        }

        public Augmentation withPosition(FilePosition position) => new Augmentation(
                this.AssociatedStatement,
                this.Locals,
                this.Fields,
                this.Parameters,
                this.InternalLocals,
                position
            );

        public CSharpSyntaxNode AssociatedStatement { get; }
        public IEnumerable<ISymbol> Locals { get; }
        public IEnumerable<ISymbol> Fields { get; }
        public IEnumerable<ISymbol> Parameters { get; }
        public IEnumerable<ISymbol> InternalLocals { get; }
        public FilePosition CurrentFilePosition { get; }

        public override bool Equals(object obj)
        {
            var augmentation = obj as Augmentation;
            return augmentation != null &&
                   EqualityComparer<CSharpSyntaxNode>.Default.Equals(AssociatedStatement, augmentation.AssociatedStatement) &&
                   EqualityComparer<IEnumerable<ISymbol>>.Default.Equals(Locals, augmentation.Locals) &&
                   EqualityComparer<IEnumerable<ISymbol>>.Default.Equals(Fields, augmentation.Fields) &&
                   EqualityComparer<IEnumerable<ISymbol>>.Default.Equals(Parameters, augmentation.Parameters) &&
                   EqualityComparer<IEnumerable<ISymbol>>.Default.Equals(InternalLocals, augmentation.InternalLocals) &&
                   EqualityComparer<FilePosition>.Default.Equals(CurrentFilePosition, augmentation.CurrentFilePosition);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(AssociatedStatement, Locals, Fields, Parameters, InternalLocals, CurrentFilePosition);
        }

        public string Serialize()
        {
            var symbolInformation = new[] {
                PrintSymbols("locals", this.Locals),
                PrintSymbols("parameters", this.Parameters),
                PrintSymbols("fields", this.Fields)
            }.Join();

            var str = $@"
    \""filePosition\"": {{
        \""line\"": {CurrentFilePosition.Line},
        \""character\"": {CurrentFilePosition.Character},
        \""file\"": \""{CurrentFilePosition.File}\""
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
