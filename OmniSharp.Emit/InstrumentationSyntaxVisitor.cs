using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;

namespace OmniSharp.Emit
{
    public class InstrumentationSyntaxVisitor : CSharpSyntaxRewriter
    {
        private readonly SemanticModel _semanticModel;

        private List<Augmentation> _augmented;

        private readonly IEnumerable<TextSpan> _replacementRegions;

        public InstrumentationSyntaxVisitor(SemanticModel semanticModel, IEnumerable<TextSpan> replacementRegions = null)
        {
            _semanticModel = semanticModel;
            _replacementRegions = replacementRegions;
        }

        public IEnumerable<Augmentation> GetAugmentations()
        {
            //TODO: note that this is *not* thread safe!
            _augmented = new List<Augmentation>();

            Visit(_semanticModel.SyntaxTree.GetRoot());

            return _augmented;
        }

        public override SyntaxNode VisitBlock(BlockSyntax node)
        {
            if (node.Statements.Count > 0)
            {
                RecordAugmentations(node.Statements);
            }

            // recurse 
            return base.VisitBlock(node);
        }

        private void RecordAugmentations(IEnumerable<StatementSyntax> statements)
        {
            // get the parent assigned variables, and static status
            // it should be the same for all statements, as we're operating at the block level here
            var parentAssigned = GetAssignedVariablesAtParent(statements.First());
            var isInStaticMethod = _semanticModel.GetEnclosingSymbol(statements.First().SpanStart).IsStatic;

            var filteredStatements = FilterStatementsByRegions(statements).ToList();
            for(int i = 0; i < filteredStatements.Count; i++)
            {
                var statement = filteredStatements[i];
                var dataFlow = i > 0 ? _semanticModel.AnalyzeDataFlow(filteredStatements[0], filteredStatements[i - 1]) : null;
                var assigned = i > 0 ? dataFlow.AlwaysAssigned.Union(parentAssigned).ToList() : parentAssigned;

                var symbols = _semanticModel.LookupSymbols(statement.FullSpan.Start);
                var locals = symbols.Where(s => s.Kind == SymbolKind.Local && assigned.Contains(s));
                var fields = symbols.Where(s => s.Kind == SymbolKind.Field && (s.IsStatic || !isInStaticMethod));
                var param = symbols.Where(s => s.Kind == SymbolKind.Parameter);



                var augmentation = new Augmentation(statement, locals, fields, param);

                this._augmented.Add(augmentation);
            }
        }

        private IEnumerable<StatementSyntax> FilterStatementsByRegions(IEnumerable<StatementSyntax> statements)
        {
            return _replacementRegions != null ? statements.Where(s => _replacementRegions.Any(r => r.OverlapsWith(s.Span))) : statements;
        }

        private IEnumerable<ISymbol> GetAssignedVariablesAtParent(StatementSyntax statement)
        {
            if (statement.Parent is BlockSyntax block)
            {
                var parentAugmentation = _augmented.SingleOrDefault(e => e.AssociatedStatement == block.Parent);
                if (parentAugmentation != null)
                {
                    return parentAugmentation.Locals;
                }
            }

            return new List<ISymbol>();
        }
    }
}
