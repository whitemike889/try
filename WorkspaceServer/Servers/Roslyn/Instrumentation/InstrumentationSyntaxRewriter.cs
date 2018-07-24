using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WorkspaceServer.Servers.Roslyn.Instrumentation
{

    public class InstrumentationSyntaxRewriter : CSharpSyntaxRewriter
    {
        public static readonly string Sentinel = "6a2f74a2-f01d-423d-a40f-726aa7358a81";

        private readonly IEnumerable<ISerializableOnce> _serializeOnce;
        private readonly IEnumerable<ISerializableEveryLine> _serializeEveryStep;
        private readonly IEnumerable<SyntaxNode> _instrumentedNodes;
        public InstrumentationSyntaxRewriter(
            IEnumerable<SyntaxNode> instrumentedNodes,
            IEnumerable<ISerializableOnce> printOnce,
            IEnumerable<ISerializableEveryLine> printEveryStep
            )
        {
            _serializeOnce = printOnce ?? throw new ArgumentNullException(nameof(printOnce));
            _serializeEveryStep = printEveryStep ?? throw new ArgumentNullException(nameof(printEveryStep));
            _instrumentedNodes = instrumentedNodes ?? throw new ArgumentNullException(nameof(instrumentedNodes));
        }

        public SyntaxTree ApplyToTree(SyntaxTree tree)
        {
            var newRoot = Visit(tree.GetRoot());
            return tree.WithRootAndOptions(newRoot, tree.Options);
        }


        public override SyntaxList<TNode> VisitList<TNode>(SyntaxList<TNode> list)
        {
            return SyntaxFactory.List(AugmentListWithInstrumentationStatments(list));
        }

        private bool IsEntryPoint(SyntaxNode node)
        {
            if (node.Parent is BlockSyntax && node.Parent.Parent is MethodDeclarationSyntax)
            {
                var method = (MethodDeclarationSyntax)node.Parent.Parent;
                return method.Identifier.Text == "Main";
            }
            else return false;
        }

        public IEnumerable<TNode> AugmentListWithInstrumentationStatments<TNode>(SyntaxList<TNode> list) where TNode : SyntaxNode
        {
            foreach (var node in list)
            {
                if (IsEntryPoint(node) && node == list.First() && _serializeOnce.Count() > 0)
                {
                    yield return (TNode)(SyntaxNode)PrintInstrumentation();
                }
                if (_instrumentedNodes.Contains(node) && _serializeEveryStep.Count() > 0)
                {
                    yield return (TNode)(SyntaxNode)PrintLineInstrumentation(node);
                }

                yield return (TNode)Visit(node);
            }
        }
        private StatementSyntax PrintLineInstrumentation(SyntaxNode node)
        {
            var data = _serializeEveryStep.Select(serializer => serializer.SerializeForLine(node))
                     .Join();
            return CreateSyntaxNode(data);
        }

        private StatementSyntax PrintInstrumentation()
        {
            var data = _serializeOnce.Select(serializer => serializer.Serialize()).Join();
            return CreateSyntaxNode(data);
        }

        private StatementSyntax CreateSyntaxNode(string instrumentationJson)
        {
            var uglifiedData = Regex.Replace(instrumentationJson, @"\r\n|\r|\n", "");
            var injectedCode = $"System.Console.WriteLine(\"{Sentinel}{{{uglifiedData}}}{Sentinel}\");";
            return SyntaxFactory.ParseStatement(injectedCode);
        }
    }
}

