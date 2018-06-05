using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace OmniSharp.Emit.Tests
{
    public class InstrumentationSyntaxVisitorTests
    {
        [Fact]
        public void Instrumentation_Is_Not_Produced_When_There_Are_No_Statements()
        {
            //arrange
            var semanticModel = Sources.GetSemanticModel(Sources.empty);
            var visitor = new InstrumentationSyntaxVisitor(semanticModel);

            //act
            var augmentations = visitor.GetAugmentations().ToList();

            //assert
            Assert.Empty(augmentations);
        }

        [Fact]
        public void Instrumentation_Is_Empty_When_There_Is_No_State()
        {
            //arrange
            var semanticModel = Sources.GetSemanticModel(Sources.simple);
            var visitor = new InstrumentationSyntaxVisitor(semanticModel);

            //act
            var augmentations = visitor.GetAugmentations().ToList();

            //assert
            Assert.Single(augmentations);
            Assert.Empty(augmentations[0].Fields);
            Assert.Empty(augmentations[0].Locals);
            Assert.Empty(augmentations[0].Parameters);
        }

        [Fact]
        public void Single_Statement_Is_Instrumented_In_Single_Statement_Program()
        {
            //arrange
            var semanticModel = Sources.GetSemanticModel(Sources.simple);
            var visitor = new InstrumentationSyntaxVisitor(semanticModel);

            //act
            var augmentations = visitor.GetAugmentations().ToList();

            //assert
            Assert.Single(augmentations);
            Assert.Equal(@"Console.WriteLine(""Entry Point"");", augmentations[0].AssociatedStatement.ToString());
        }

        [Fact]
        public void Multiple_Statements_Are_Instrumented_In_Multiple_Statement_Program()
        {
            //arrange
            var semanticModel = Sources.GetSemanticModel(Sources.withLocalsAndParams);
            var visitor = new InstrumentationSyntaxVisitor(semanticModel);

            //act
            var augmentations = visitor.GetAugmentations().ToList();

            //assert
            Assert.Equal(2, augmentations.Count);
            Assert.Equal(@"int a = 0;", augmentations[0].AssociatedStatement.ToString());
            Assert.Equal(@"Console.WriteLine(""Entry Point"");", augmentations[1].AssociatedStatement.ToString());
        }

        [Fact]
        public void Only_Requested_Statements_Are_Instrumented_When_Regions_Are_Supplied()
        {
            //arrange
            var semanticModel = Sources.GetSemanticModel(Sources.withMultipleMethodsAndComplexLayout);
            var regions = new List<TextSpan>() { new TextSpan(169, 84) };
            var visitor = new InstrumentationSyntaxVisitor(semanticModel, regions);

            //act
            var augmentations = visitor.GetAugmentations().ToList();

            //assert
            Assert.Equal(2, augmentations.Count);
            Assert.Equal(@"Console.WriteLine(""Entry Point"");", augmentations[0].AssociatedStatement.ToString());
            Assert.Equal(@"var p = new Program();", augmentations[1].AssociatedStatement.ToString());
        }

        [Fact]
        public void Only_Requested_Statements_Are_Instrumented_When_Non_Contiguous_Regions_Are_Supplied()
        {
            //arrange
            var semanticModel = Sources.GetSemanticModel(Sources.withMultipleMethodsAndComplexLayout);
            var regions = new List<TextSpan>() { new TextSpan(156, 35), new TextSpan(476, 32) };
            var visitor = new InstrumentationSyntaxVisitor(semanticModel, regions);

            //act
            var augmentations = visitor.GetAugmentations().ToList();

            //assert
            Assert.Equal(2, augmentations.Count);
            Assert.Equal(@"Console.WriteLine(""Entry Point"");", augmentations[0].AssociatedStatement.ToString());
            Assert.Equal(@"Console.WriteLine(""Instance"");", augmentations[1].AssociatedStatement.ToString());
        }

        [Fact]
        public void Locals_Are_Captured()
        {
            //arrange
            var semanticModel = Sources.GetSemanticModel(Sources.withLocalsAndParams);
            var visitor = new InstrumentationSyntaxVisitor(semanticModel);

            //act
            var augmentations = visitor.GetAugmentations().ToList();

            //assert
            Assert.Single(augmentations[1].Locals);
            Assert.Contains(augmentations[1].Locals, l => l.Name == "a");
        }

        [Fact]
        public void Locals_Are_Captured_After_Being_Assigned()
        {
            //arrange
            var semanticModel = Sources.GetSemanticModel(Sources.withNonAssignedLocals);
            var visitor = new InstrumentationSyntaxVisitor(semanticModel);

            //act
            var augmentations = visitor.GetAugmentations().ToList();

            //assert
            Assert.Single(augmentations[3].Locals);
            Assert.Equal(2, augmentations[4].Locals.Count());
            Assert.Contains(augmentations[4].Locals, l => l.Name == "s");
        }

        [Fact]
        public void Locals_Are_Not_Captured_Before_Being_Assigned()
        {
            //arrange
            var semanticModel = Sources.GetSemanticModel(Sources.withNonAssignedLocals);
            var visitor = new InstrumentationSyntaxVisitor(semanticModel);

            //act
            var augmentations = visitor.GetAugmentations().ToList();

            //assert
            Assert.Empty(augmentations[1].Locals);
            Assert.Single(augmentations[2].Locals);
            Assert.Contains(augmentations[2].Locals, l => l.Name == "a");
        }

        [Fact]
        public void Locals_Are_Captured_Based_On_Scope()
        {
            //arrange
            var semanticModel = Sources.GetSemanticModel(Sources.withMultipleMethodsAndComplexLayout);
            var visitor = new InstrumentationSyntaxVisitor(semanticModel);

            //act
            var augmentations = visitor.GetAugmentations().ToList();

            //assert
            Assert.NotEmpty(augmentations[6].Locals);
            Assert.Contains(augmentations[6].Locals, l => l.Name == "j");
            Assert.DoesNotContain(augmentations[6].Locals, l => l.Name == "k");

        }

        [Fact]
        public void Parameters_Are_Captured()
        {
            //arrange
            var semanticModel = Sources.GetSemanticModel(Sources.withLocalsAndParams);
            InstrumentationSyntaxVisitor visitor = new InstrumentationSyntaxVisitor(semanticModel);

            //act
            var augmentations = visitor.GetAugmentations().ToList();

            //assert
            Assert.Single(augmentations[0].Parameters);
            Assert.Contains(augmentations[0].Parameters, p => p.Name == "args");
        }

        [Fact]
        public void Static_Fields_Are_Captured_In_Static_Methods()
        {
            //arrange
            var semanticModel = Sources.GetSemanticModel(Sources.withStaticAndNonStaticField);
            InstrumentationSyntaxVisitor visitor = new InstrumentationSyntaxVisitor(semanticModel);

            //act
            var augmentations = visitor.GetAugmentations().ToList();

            //assert
            Assert.Single(augmentations[0].Fields);
            Assert.Contains(augmentations[0].Fields, f => f.Name == "a");
        }

        [Fact]
        public void Static_Fields_Are_Captured_In_Instance_Methods()
        {
            //arrange
            var semanticModel = Sources.GetSemanticModel(Sources.withStaticAndNonStaticField);
            InstrumentationSyntaxVisitor visitor = new InstrumentationSyntaxVisitor(semanticModel);

            //act
            var augmentations = visitor.GetAugmentations().ToList();

            //assert
            Assert.NotEmpty(augmentations[1].Fields);
            Assert.Contains(augmentations[1].Fields, f => f.Name == "a");
        }

        [Fact]
        public void Instance_Fields_Are_Captured_In_Instance_Methods()
        {
            //arrange
            var semanticModel = Sources.GetSemanticModel(Sources.withStaticAndNonStaticField);
            InstrumentationSyntaxVisitor visitor = new InstrumentationSyntaxVisitor(semanticModel);

            //act
            var augmentations = visitor.GetAugmentations().ToList();

            //assert
            Assert.NotEmpty(augmentations[1].Fields);
            Assert.Contains(augmentations[1].Fields, f => f.Name == "b");
        }

        [Fact]
        public void Instance_Fields_Are_Not_Captured_In_Static_Methods()
        {
            //arrange
            var semanticModel = Sources.GetSemanticModel(Sources.withStaticAndNonStaticField);
            InstrumentationSyntaxVisitor visitor = new InstrumentationSyntaxVisitor(semanticModel);

            //act
            var augmentations = visitor.GetAugmentations().ToList();

            //assert
            Assert.Single(augmentations[0].Fields);
            Assert.DoesNotContain(augmentations[0].Fields, f => f.Name == "b");
        }
    }
}
