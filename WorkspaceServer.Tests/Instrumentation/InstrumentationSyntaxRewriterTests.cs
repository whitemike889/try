using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using WorkspaceServer.Servers.Roslyn.Instrumentation;
using Xunit;

namespace WorkspaceServer.Tests.Servers.Roslyn.Instrumentation
{
    public class InstrumentationSyntaxRewriterTests
    {
        [Fact]
        public void Rewritten_Code_With_Augmentations_Has_Calls_To_EmitProgramState()
        {

        }

        [Fact]
        public void Rewritten_Code_Has_Calls_To_GetProgramState()
        {

        }
        
        [Fact]
        public void Rewritten_Code_Is_Not_Modified_If_No_Augmentations()
        {

        }

        [Fact]
        public void Syntax_Tree_Is_Unchanged_When_Given_No_Augmentations()
        {
            // arrange
            var document = Sources.GetDocument(Sources.simple);
            var syntaxTree = document.GetSyntaxTreeAsync().Result;
            var rewriter = new InstrumentationSyntaxRewriter
                (
                Enumerable.Empty<SyntaxNode>(),
                Enumerable.Empty<ISerializableOnce>(),
                Enumerable.Empty<ISerializableEveryLine>()
                );

            // act
            var newTree = rewriter.ApplyToTree(syntaxTree);

            // assert
            Assert.True(syntaxTree.IsEquivalentTo(newTree));
        }

        [Fact]
        public void Syntax_Tree_Has_A_Single_Extra_Statement_When_There_Is_One_Augmentation()
        {
            // arrange
            var document = Sources.GetDocument(Sources.simple);
            var syntaxTree = document.GetSyntaxTreeAsync().Result;
            var statementCount = syntaxTree.GetRoot().DescendantNodes().Count(n => n is StatementSyntax);
            var statement = (StatementSyntax)syntaxTree.GetRoot().DescendantNodes().Single(n => n.ToString() == @"Console.WriteLine(""Entry Point"");");

            var augmentation = new Augmentation(statement, null, null, null, null);
            var augMap = new AugmentationMap(augmentation);

            var rewriter = new InstrumentationSyntaxRewriter(
                augMap.Data.Keys,
                Enumerable.Empty<ISerializableOnce>(),
                new[] { augMap }
                );

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
            var document = Sources.GetDocument(Sources.withMultipleMethodsAndComplexLayout);
            var syntaxTree = document.GetSyntaxTreeAsync().Result;
            var augmentations = syntaxTree.GetRoot()
                .DescendantNodes()
                .Where(n => n is StatementSyntax)
                .Select(n => new Augmentation((StatementSyntax)n, null, null, null, null));
            var augMap = new AugmentationMap(augmentations.ToArray());

            var rewriter = new InstrumentationSyntaxRewriter(
                augMap.Data.Keys,
                Enumerable.Empty<ISerializableOnce>(),
                new[] { augMap }
                );

            // act
            var newTree = rewriter.ApplyToTree(syntaxTree);
            var newStatementCount = newTree.GetRoot().DescendantNodes().Count(n => n is StatementSyntax);

            // assert
            Assert.Equal(24, newStatementCount);
        }

        [Fact]
        public void Syntax_Tree_Has_Locals_When_Augmentation_Has_Locals()
        {
            // arrange
            var document = Sources.GetDocument(Sources.withMultipleMethodsAndComplexLayout);
            var syntaxTree = document.GetSyntaxTreeAsync().Result;
            var statementCount = syntaxTree.GetRoot().DescendantNodes().Count(n => n is StatementSyntax);
            var statement = (StatementSyntax)syntaxTree.GetRoot().DescendantNodes().Single(n => n.ToString() == @"Console.WriteLine(""Entry Point"");");

            var locals = document.GetSemanticModelAsync().Result.LookupSymbols(310).Where(s => s.Kind == Microsoft.CodeAnalysis.SymbolKind.Local);
            var augmentations = new[] { new Augmentation(statement, locals, null, null, null) };

            var augMap = new AugmentationMap(augmentations.ToArray());
            var rewriter = new InstrumentationSyntaxRewriter(
                augMap.Data.Keys,
                Enumerable.Empty<ISerializableOnce>(),
                new[] { augMap }
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
        public void Syntax_Tree_Has_Fields_When_Augmentation_Has_Fields()
        {
            // arrange
            var document = Sources.GetDocument(Sources.withMultipleMethodsAndComplexLayout);
            var syntaxTree = document.GetSyntaxTreeAsync().Result;
            var statementCount = syntaxTree.GetRoot().DescendantNodes().Count(n => n is StatementSyntax);
            var statement = (StatementSyntax)syntaxTree.GetRoot().DescendantNodes().Single(n => n.ToString() == @"Console.WriteLine(""Entry Point"");");
            var fields = document.GetSemanticModelAsync().Result.LookupSymbols(310).Where(s => s.Kind == Microsoft.CodeAnalysis.SymbolKind.Field);
            var augmentations = new[] { new Augmentation(statement, null, fields, null, null) };
            var augMap = new AugmentationMap(augmentations.ToArray());
            var rewriter = new InstrumentationSyntaxRewriter(
                 augMap.Data.Keys,
                 Enumerable.Empty<ISerializableOnce>(),
                 new[] { augMap }
                 );

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
            var document = Sources.GetDocument(Sources.withMultipleMethodsAndComplexLayout);
            var syntaxTree = document.GetSyntaxTreeAsync().Result;
            var statementCount = syntaxTree.GetRoot().DescendantNodes().Count(n => n is StatementSyntax);
            var statement = (StatementSyntax)syntaxTree.GetRoot().DescendantNodes().Single(n => n.ToString() == @"Console.WriteLine(""Entry Point"");");
            var parameters = document.GetSemanticModelAsync().Result.LookupSymbols(310).Where(s => s.Kind == Microsoft.CodeAnalysis.SymbolKind.Parameter);
            var augmentations = new[] { new Augmentation(statement, null, null, parameters, null) };
            var augMap = new AugmentationMap(augmentations.ToArray());
            var rewriter = new InstrumentationSyntaxRewriter(
                             augMap.Data.Keys,
                             Enumerable.Empty<ISerializableOnce>(),
                             new[] { augMap }
                             );
            // act
            var newTree = rewriter.ApplyToTree(syntaxTree);
            var treeString = newTree.ToString();

            // assert
            Assert.Contains("\\\"name\\\": \\\"args\\\"", treeString);
        }
    }
}

