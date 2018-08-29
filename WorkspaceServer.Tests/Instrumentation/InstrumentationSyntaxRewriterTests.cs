using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using WorkspaceServer.Servers.Roslyn.Instrumentation;
using WorkspaceServer.Tests.Servers.Roslyn.Instrumentation;
using Xunit;

namespace WorkspaceServer.Tests.Instrumentation
{
    public class InstrumentationSyntaxRewriterTests
    {
        [Fact]
        public async Task Syntax_Tree_Is_Unchanged_When_Given_No_Augmentations()
        {
            // arrange
            var document = Sources.GetDocument(Sources.simple);
            var syntaxTree = await document.GetSyntaxTreeAsync();
            var rewriter = new InstrumentationSyntaxRewriter
                (
                Enumerable.Empty<SyntaxNode>(),
                new VariableLocationMap(),
                new AugmentationMap()
                );

            // act
            var newTree = rewriter.ApplyToTree(syntaxTree);

            // assert
            Assert.True(syntaxTree.IsEquivalentTo(newTree));
        }

        [Fact]
        public async Task Syntax_Tree_Has_A_Single_Extra_Statement_When_There_Is_One_Augmentation()
        {
            // arrange
            var document = Sources.GetDocument(Sources.simple);
            var syntaxTree = await document.GetSyntaxTreeAsync();
            var statementCount = syntaxTree.GetRoot().DescendantNodes().Count(n => n is StatementSyntax);
            var statement = (StatementSyntax)syntaxTree.GetRoot().DescendantNodes().Single(n => n.ToString() == @"Console.WriteLine(""Entry Point"");");

            var augmentation = new Augmentation(statement, null, null, null, null);
            var augMap = new AugmentationMap(augmentation);

            var rewriter = new InstrumentationSyntaxRewriter(
                augMap.Data.Keys,
                new VariableLocationMap(),
                augMap 
                );

            // act
            var newTree = rewriter.ApplyToTree(syntaxTree);
            var newStatementCount = newTree.GetRoot().DescendantNodes().Count(n => n is StatementSyntax);

            // assert
            Assert.Equal(statementCount + 1, newStatementCount);
        }

        [Fact]
        public async Task Syntax_Tree_Has_Extra_Statements_When_Everything_Is_Augmented()
        {
            // arrange
            var document = Sources.GetDocument(Sources.withMultipleMethodsAndComplexLayout);
            var syntaxTree = await document.GetSyntaxTreeAsync();
            var augmentations = syntaxTree.GetRoot()
                .DescendantNodes()
                .Where(n => n is StatementSyntax)
                .Select(n => new Augmentation((StatementSyntax)n, null, null, null, null));
            var augMap = new AugmentationMap(augmentations.ToArray());

            var rewriter = new InstrumentationSyntaxRewriter(
                augMap.Data.Keys,
                new VariableLocationMap(),
                augMap
                );

            // act
            var newTree = rewriter.ApplyToTree(syntaxTree);
            var newStatementCount = newTree.GetRoot().DescendantNodes().Count(n => n is StatementSyntax);

            // assert
            Assert.Equal(24, newStatementCount);
        }

        [Fact]
        public async Task Syntax_Tree_Has_Locals_When_Augmentation_Has_Locals()
        {
            // arrange
            var document = Sources.GetDocument(Sources.withMultipleMethodsAndComplexLayout);
            var syntaxTree = await document.GetSyntaxTreeAsync();
            var statementCount = syntaxTree.GetRoot().DescendantNodes().Count(n => n is StatementSyntax);
            var statement = (StatementSyntax)syntaxTree.GetRoot().DescendantNodes().Single(n => n.ToString() == @"Console.WriteLine(""Entry Point"");");

            var locals = (await document.GetSemanticModelAsync()).LookupSymbols(310).Where(s => s.Kind == Microsoft.CodeAnalysis.SymbolKind.Local);
            var augmentations = new[] { new Augmentation(statement, locals, null, null, null) };

            var augMap = new AugmentationMap(augmentations.ToArray());
            var rewriter = new InstrumentationSyntaxRewriter(
                augMap.Data.Keys,
                new VariableLocationMap(),
                augMap
                );

            // act
            var newTree = rewriter.ApplyToTree(syntaxTree);
            var treeString = newTree.ToString();

            // assert
            Assert.Contains("\\\"name\\\": \\\"j\\\"", treeString);
            Assert.Contains("\\\"name\\\": \\\"k\\\"", treeString);
            Assert.Contains("\\\"name\\\": \\\"p\\\"", treeString);
        }

        [Fact]
        public async Task Syntax_Tree_Has_Fields_When_Augmentation_Has_Fields()
        {
            // arrange
            var document = Sources.GetDocument(Sources.withMultipleMethodsAndComplexLayout);
            var syntaxTree = await document.GetSyntaxTreeAsync();
            var statementCount = syntaxTree.GetRoot().DescendantNodes().Count(n => n is StatementSyntax);
            var statement = (StatementSyntax)syntaxTree.GetRoot().DescendantNodes().Single(n => n.ToString() == @"Console.WriteLine(""Entry Point"");");
            var fields = (await document.GetSemanticModelAsync()).LookupSymbols(310).Where(s => s.Kind == Microsoft.CodeAnalysis.SymbolKind.Field);
            var augmentations = new[] { new Augmentation(statement, null, fields, null, null) };
            var augMap = new AugmentationMap(augmentations.ToArray());
            var rewriter = new InstrumentationSyntaxRewriter(
                 augMap.Data.Keys,
                 new VariableLocationMap(),
                 augMap
                 );

            // act
            var newTree = rewriter.ApplyToTree(syntaxTree);
            var treeString = newTree.ToString();

            // assert
            Assert.Contains("\\\"name\\\": \\\"a\\\"", treeString);
            Assert.Contains("\\\"name\\\": \\\"b\\\"", treeString);
        }

        [Fact]
        public async Task Syntax_Tree_Has_Parameters_When_Augmentation_Has_Parameters()
        {
            // arrange
            var document = Sources.GetDocument(Sources.withMultipleMethodsAndComplexLayout);
            var syntaxTree = await document.GetSyntaxTreeAsync();
            var statementCount = syntaxTree.GetRoot().DescendantNodes().Count(n => n is StatementSyntax);
            var statement = (StatementSyntax)syntaxTree.GetRoot().DescendantNodes().Single(n => n.ToString() == @"Console.WriteLine(""Entry Point"");");
            var parameters = (await document.GetSemanticModelAsync()).LookupSymbols(310).Where(s => s.Kind == Microsoft.CodeAnalysis.SymbolKind.Parameter);
            var augmentations = new[] { new Augmentation(statement, null, null, parameters, null) };
            var augMap = new AugmentationMap(augmentations.ToArray());
            var rewriter = new InstrumentationSyntaxRewriter(
                             augMap.Data.Keys,
                             new VariableLocationMap(),
                             augMap
                             );
            // act
            var newTree = rewriter.ApplyToTree(syntaxTree);
            var treeString = newTree.ToString();

            // assert
            Assert.Contains("\\\"name\\\": \\\"args\\\"", treeString);
        }
    }
}

