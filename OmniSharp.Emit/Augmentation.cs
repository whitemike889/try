using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

namespace OmniSharp.Emit
{
    public class Augmentation
    {
        public Augmentation(StatementSyntax associatedStatment, IEnumerable<ISymbol> locals, IEnumerable<ISymbol> fields, IEnumerable<ISymbol> parameters)
        {
            AssociatedStatement = associatedStatment ?? throw new ArgumentNullException(nameof(associatedStatment));
            Locals = locals ?? Array.Empty<ISymbol>();
            Fields = fields ?? Array.Empty<ISymbol>();
            Parameters = parameters ?? Array.Empty<ISymbol>();
        }

        public StatementSyntax AssociatedStatement { get; }

        public IEnumerable<ISymbol> Locals { get; }

        public IEnumerable<ISymbol> Fields { get; }

        public IEnumerable<ISymbol> Parameters { get; }


    }
}
