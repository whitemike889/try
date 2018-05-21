using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using Xunit;

namespace OmniSharp.Emit.Tests
{
    public class InstrumentationSyntaxRewriterTests
    {
        [Fact]
        public void Syntax_Tree_Is_Unchanged_When_Given_No_Augmentations()
        {
            // arrange
            var semanticModel = Sources.GetSemanticModel(Sources.simple);
            var syntaxTree = semanticModel.Compilation.SyntaxTrees.First();
            var augmentations = Array.Empty<Augmentation>();
            var rewriter = new InstrumentationSyntaxRewriter(augmentations);

            // act
            var newTree = rewriter.ApplyToTree(syntaxTree);

            // assert
            Assert.Equal(newTree, syntaxTree);
        }

        [Fact]
        public void Syntax_Tree_Has_A_Single_Extra_Statement_When_There_Is_One_Agumentation()
        {
            // arrange
            var semanticModel = Sources.GetSemanticModel(Sources.simple);
            var syntaxTree = semanticModel.Compilation.SyntaxTrees.First();
            var statementCount = syntaxTree.GetRoot().DescendantNodes().Count(n => n is StatementSyntax);
            var statement = (StatementSyntax)syntaxTree.GetRoot().DescendantNodes().Single(n => n.ToString() == @"Console.WriteLine(""Entry Point"");");
            var augmentations = new[] { new Augmentation(statement, null, null, null) };
            var rewriter = new InstrumentationSyntaxRewriter(augmentations);

            // act
            var newTree = rewriter.ApplyToTree(syntaxTree);
            var newStatementCount = newTree.GetRoot().DescendantNodes().Count(n => n is StatementSyntax);

            // assert
            Assert.Equal(statementCount + 1, newStatementCount);
        }

        [Fact]
        public void Syntax_Tree_Has_Extra_Statements_When_Everything_Is_Augmented()
        {
            // arrange
            var semanticModel = Sources.GetSemanticModel(Sources.withMultipleMethodsAndComplexLayout);
            var syntaxTree = semanticModel.Compilation.SyntaxTrees.First();
            var augmentations = syntaxTree.GetRoot().DescendantNodes().Where(n => n is StatementSyntax).Select(n => new Augmentation((StatementSyntax)n, null, null, null));
            var rewriter = new InstrumentationSyntaxRewriter(augmentations);

            // act
            var newTree = rewriter.ApplyToTree(syntaxTree);
            var newStatementCount = newTree.GetRoot().DescendantNodes().Count(n => n is StatementSyntax);

            // assert
            Assert.Equal(19, newStatementCount);
        }

        [Fact]
        public void Syntax_Tree_Has_Locals_When_Augmentation_Has_Locals()
        {
            // arrange
            var semanticModel = Sources.GetSemanticModel(Sources.withMultipleMethodsAndComplexLayout);
            var syntaxTree = semanticModel.Compilation.SyntaxTrees.First();
            var statementCount = syntaxTree.GetRoot().DescendantNodes().Count(n => n is StatementSyntax);
            var statement = (StatementSyntax)syntaxTree.GetRoot().DescendantNodes().Single(n => n.ToString() == @"Console.WriteLine(""Entry Point"");");
            var locals = semanticModel.LookupSymbols(310).Where(s => s.Kind == Microsoft.CodeAnalysis.SymbolKind.Local);
            var augmentations = new[] { new Augmentation(statement, locals, null, null) };
            var rewriter = new InstrumentationSyntaxRewriter(augmentations);

            // act
            var newTree = rewriter.ApplyToTree(syntaxTree);
            var treeString = newTree.ToString();

            // assert
            Assert.Contains("\\\"name\\\": \\\"j\\\"", treeString);
            Assert.Contains("\\\"name\\\": \\\"k\\\"", treeString);
            Assert.Contains("\\\"name\\\": \\\"p\\\"", treeString);
        }


        [Fact]
        public void Syntax_Tree_Has_Fields_When_Augmentation_Has_Fields()
        {
            // arrange
            var semanticModel = Sources.GetSemanticModel(Sources.withMultipleMethodsAndComplexLayout);
            var syntaxTree = semanticModel.Compilation.SyntaxTrees.First();
            var statementCount = syntaxTree.GetRoot().DescendantNodes().Count(n => n is StatementSyntax);
            var statement = (StatementSyntax)syntaxTree.GetRoot().DescendantNodes().Single(n => n.ToString() == @"Console.WriteLine(""Entry Point"");");
            var fields = semanticModel.LookupSymbols(310).Where(s => s.Kind == Microsoft.CodeAnalysis.SymbolKind.Field);
            var augmentations = new[] { new Augmentation(statement, null, fields, null) };
            var rewriter = new InstrumentationSyntaxRewriter(augmentations);

            // act
            var newTree = rewriter.ApplyToTree(syntaxTree);
            var treeString = newTree.ToString();

            // assert
            Assert.Contains("\\\"name\\\": \\\"a\\\"", treeString);
            Assert.Contains("\\\"name\\\": \\\"b\\\"", treeString);
        }

        [Fact]
        public void Syntax_Tree_Has_Parameters_When_Augmentation_Has_Parameters()
        {
            // arrange
            var semanticModel = Sources.GetSemanticModel(Sources.withMultipleMethodsAndComplexLayout);
            var syntaxTree = semanticModel.Compilation.SyntaxTrees.First();
            var statementCount = syntaxTree.GetRoot().DescendantNodes().Count(n => n is StatementSyntax);
            var statement = (StatementSyntax)syntaxTree.GetRoot().DescendantNodes().Single(n => n.ToString() == @"Console.WriteLine(""Entry Point"");");
            var parameters = semanticModel.LookupSymbols(310).Where(s => s.Kind == Microsoft.CodeAnalysis.SymbolKind.Parameter);
            var augmentations = new[] { new Augmentation(statement, null, null, parameters) };
            var rewriter = new InstrumentationSyntaxRewriter(augmentations);

            // act
            var newTree = rewriter.ApplyToTree(syntaxTree);
            var treeString = newTree.ToString();

            // assert
            Assert.Contains("\\\"name\\\": \\\"args\\\"", treeString);
        }
    }
}
