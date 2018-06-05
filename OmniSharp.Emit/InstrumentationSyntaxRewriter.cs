using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OmniSharp.Emit
{

    public class InstrumentationSyntaxRewriter : CSharpSyntaxRewriter
    {
        public static readonly string Sentinel = "6a2f74a2-f01d-423d-a40f-726aa7358a81";

        private readonly IEnumerable<Augmentation> _augmentations;

        public InstrumentationSyntaxRewriter(IEnumerable<Augmentation> augmentations)
        {
            _augmentations = augmentations ?? Array.Empty<Augmentation>();
        }

        public SyntaxTree ApplyToTree(SyntaxTree tree)
        {
            if (_augmentations?.Count() == 0)
            {
                return tree;
            }

            var newRoot = Visit(tree.GetRoot());
            return tree.WithRootAndOptions(newRoot, tree.Options);
        }

        public override SyntaxList<TNode> VisitList<TNode>(SyntaxList<TNode> list)
        {
            return SyntaxFactory.List(AugmentListWithInstrumentationStatments(list));
        }

        public IEnumerable<TNode> AugmentListWithInstrumentationStatments<TNode>(SyntaxList<TNode> list) where TNode : SyntaxNode
        {
            foreach (var node in list)
            {
                var augmentation = _augmentations.SingleOrDefault(e => e.AssociatedStatement == node); 
                if (augmentation != null)
                {
                    yield return (TNode)(SyntaxNode)GetInstrumentationSyntax(augmentation);
                }

                yield return (TNode)Visit(node);
            }
        }

        static StatementSyntax GetInstrumentationSyntax(Augmentation augmentation)
        {
            // hmm... building json by hand :)
            var sb = new StringBuilder();
            sb.Append("{");

            // position
            var position = augmentation.AssociatedStatement.GetLocation().GetLineSpan();
            sb.Append("\\\"filePosition\\\": {");
            sb.Append($"\\\"line\\\": {position.StartLinePosition.Line}, ");
            sb.Append($"\\\"character\\\": {position.StartLinePosition.Character}, ");
            sb.Append($"\\\"file\\\": \\\"{Path.GetFileName(position.Path)}\\\"");
            sb.Append("},");

            // stack trace
            sb.Append("\\\"stackTrace\\\": ");
            sb.Append("\\\" \"+ new System.Diagnostics.StackTrace(false).ToString() +\" \\\"");
            sb.Append(",");

            // symbols
            PrintSymbols(sb, "locals", augmentation.Locals);
            PrintSymbols(sb, "parameters", augmentation.Parameters);
            PrintSymbols(sb, "fields", augmentation.Fields);

            sb.Append("}");

            //Sentinal
            sb.Insert(0, $"{Sentinel}");
            sb.Append($"{Sentinel}");
            return SyntaxFactory.ParseStatement("System.Console.WriteLine(\"" + sb.ToString() + "\");");

        }

        private static void PrintSymbols(StringBuilder sb, string symbolType, IEnumerable<ISymbol> symbols)
        {
            sb.Append($"\\\"{symbolType}\\\" : [ ");
            foreach (var symbol in symbols)
            {
                var location = symbol.DeclaringSyntaxReferences.First().Span;

                sb.Append("{");
                sb.Append($"\\\"name\\\": \\\"{symbol.Name}\\\", ");
                sb.Append($"\\\"value\\\": \\\"\"+({symbol.Name} != null ? System.Uri.EscapeDataString({symbol.Name}.ToString()) : \"null\")+\"\\\", ");
                sb.Append($"\\\"declaredAt\\\" :{{ \\\"start\\\" : {location.Start}, \\\"end\\\" : {location.End} }}");
                sb.Append("},");
            }
            sb.Append("],");
        }
    }
}
