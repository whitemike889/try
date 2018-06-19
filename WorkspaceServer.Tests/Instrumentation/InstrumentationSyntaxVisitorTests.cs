using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace WorkspaceServer.Servers.Roslyn.Instrumentation.Tests
{
    public class InstrumentationSyntaxVisitorTests
    {
        private AugmentationMap GetAugmentationMap(string source, IEnumerable<TextSpan> regions = null)
        {
            var document = Sources.GetDocument(source, true);
            if (regions == null)
            {
                return new InstrumentationSyntaxVisitor(document).Augmentations;
            }
            else
            {
                return new InstrumentationSyntaxVisitor(document, regions).Augmentations;
            }
        }

        [Fact]
        public void Instrumentation_Is_Not_Produced_When_There_Are_No_Statements()
        {
            var augmentations = GetAugmentationMap(Sources.empty).Data;
            Assert.Empty(augmentations);
        }

        [Fact]
        public void Instrumentation_Is_Empty_When_There_Is_No_State()
        {
            var augmentations = GetAugmentationMap(Sources.simple).Data.Values.ToList();

            //assert
            Assert.Single(augmentations);
            Assert.Empty(augmentations[0].Fields);
            Assert.Empty(augmentations[0].Locals);
            Assert.Empty(augmentations[0].Parameters);
        }

        [Fact]
        public void Single_Statement_Is_Instrumented_In_Single_Statement_Program()
        {
            //act
            var augmentations = GetAugmentationMap(Sources.simple).Data.Values.ToList();

            //assert
            Assert.Single(augmentations);
            Assert.Equal(@"Console.WriteLine(""Entry Point"");", augmentations[0].AssociatedStatement.ToString());
        }

        [Fact]
        public void Multiple_Statements_Are_Instrumented_In_Multiple_Statement_Program()
        {
            //act
            var augmentations = GetAugmentationMap(Sources.withLocalsAndParams).Data.Values.ToList();

            //assert
            Assert.Equal(2, augmentations.Count);
            Assert.Equal(@"int a = 0;", augmentations[0].AssociatedStatement.ToString());
            Assert.Equal(@"Console.WriteLine(""Entry Point"");", augmentations[1].AssociatedStatement.ToString());
        }


        [Fact]
        public void Only_Requested_Statements_Are_Instrumented_When_Regions_Are_Supplied()
        {
            //arrange
            var regions = new List<TextSpan>() { new TextSpan(169, 84) };

            //act
            var augmentations = GetAugmentationMap(Sources.withMultipleMethodsAndComplexLayout, regions).Data.Values.ToList();

            //assert
            Assert.Equal(2, augmentations.Count);
            Assert.Equal(@"Console.WriteLine(""Entry Point"");", augmentations[0].AssociatedStatement.ToString());
            Assert.Equal(@"var p = new Program();", augmentations[1].AssociatedStatement.ToString());
        }

        [Fact]
        public void Only_Requested_Statements_Are_Instrumented_When_Non_Contiguous_Regions_Are_Supplied()
        {
            //arrange
            var regions = new List<TextSpan>() { new TextSpan(156, 35), new TextSpan(625, 32) };

            //act
            var augmentations = GetAugmentationMap(Sources.withMultipleMethodsAndComplexLayout, regions).Data.Values.ToList();

            //assert
            Assert.Equal(2, augmentations.Count);
            Assert.Equal(@"Console.WriteLine(""Entry Point"");", augmentations[0].AssociatedStatement.ToString());
            Assert.Equal(@"Console.WriteLine(""Instance"");", augmentations[1].AssociatedStatement.ToString());
        }

        [Fact]
        public void Locals_Are_Captured()
        {
            //act
            var augmentations = GetAugmentationMap(Sources.withLocalsAndParams).Data.Values.ToList();

            //assert
            Assert.Single(augmentations[1].Locals);
            Assert.Contains(augmentations[1].Locals, l => l.Name == "a");
        }

        [Fact]
        public void Locals_Are_Captured_After_Being_Assigned()
        {
            //act
            var augmentations = GetAugmentationMap(Sources.withNonAssignedLocals).Data.Values.ToList();

            //assert
            Assert.Single(augmentations[3].Locals);
            Assert.Equal(2, augmentations[4].Locals.Count());
            Assert.Contains(augmentations[4].Locals, l => l.Name == "s");
        }

        [Fact]
        public void Locals_Are_Not_Captured_Before_Being_Assigned()
        {
            //act
            var augmentations = GetAugmentationMap(Sources.withNonAssignedLocals).Data.Values.ToList();

            //assert
            Assert.Empty(augmentations[1].Locals);
            Assert.Single(augmentations[2].Locals);
            Assert.Contains(augmentations[2].Locals, l => l.Name == "a");
        }

        [Fact]
        public void Locals_Are_Captured_Based_On_Scope()
        {
            //act
            var augmentations = GetAugmentationMap(Sources.withMultipleMethodsAndComplexLayout).Data.Values.ToList();

            //assert
            Assert.NotEmpty(augmentations[6].Locals);
            Assert.Contains(augmentations[6].Locals, l => l.Name == "j");
            Assert.DoesNotContain(augmentations[6].Locals, l => l.Name == "k");

        }

        [Fact]
        public void RangeVariables_Are_Captured_As_Locals_Inside_Loops()
        {
            //act
            var augmentations = GetAugmentationMap(Sources.withMultipleMethodsAndComplexLayout).Data.Values.ToList();

            //assert
            Assert.NotEmpty(augmentations[7].Locals);
            Assert.Contains(augmentations[7].Locals, l => l.Name == "i");
            Assert.DoesNotContain(augmentations[5].Locals, l => l.Name == "i");
            Assert.DoesNotContain(augmentations[6].Locals, l => l.Name == "i");
        }


        [Fact]
        public void ForEachVariables_Are_Captured_As_Locals_Inside_Loops()
        {
            //act
            var augmentations = GetAugmentationMap(Sources.withMultipleMethodsAndComplexLayout).Data.Values.ToList();

            //assert
            Assert.NotEmpty(augmentations[8].Locals);
            Assert.Contains(augmentations[8].Locals, l => l.Name == "number");
            Assert.DoesNotContain(augmentations[6].Locals, l => l.Name == "i");
            Assert.DoesNotContain(augmentations[7].Locals, l => l.Name == "number");
        }


        [Fact]
        public void Parameters_Are_Captured()
        {
            var augmentations = GetAugmentationMap(Sources.withLocalsAndParams).Data.Values.ToList();

            //assert
            Assert.Single(augmentations[0].Parameters);
            Assert.Contains(augmentations[0].Parameters, p => p.Name == "args");
        }

        [Fact]
        public void Static_Fields_Are_Captured_In_Static_Methods()
        {
            //arrange
            var document = Sources.GetDocument(Sources.withStaticAndNonStaticField, true);
            InstrumentationSyntaxVisitor visitor = new InstrumentationSyntaxVisitor(document);

            //act
            var augmentations = visitor.Augmentations.Data.Values.ToList();

            //assert
            Assert.Single(augmentations[0].Fields);
            Assert.Contains(augmentations[0].Fields, f => f.Name == "a");
        }

        [Fact]
        public void Static_Fields_Are_Captured_In_Instance_Methods()
        {
            //arrange
            var augmentations = GetAugmentationMap(Sources.withStaticAndNonStaticField).Data.Values.ToList();


            //assert
            Assert.NotEmpty(augmentations[1].Fields);
            Assert.Contains(augmentations[1].Fields, f => f.Name == "a");
        }

        [Fact]
        public void Instance_Fields_Are_Captured_In_Instance_Methods()
        {
            //arrange
            var augmentations = GetAugmentationMap(Sources.withStaticAndNonStaticField).Data.Values.ToList();

            //assert
            Assert.NotEmpty(augmentations[1].Fields);
            Assert.Contains(augmentations[1].Fields, f => f.Name == "b");
        }

        [Fact]
        public void Instance_Fields_Are_Not_Captured_In_Static_Methods()
        {
            //arrange
            var augmentations = GetAugmentationMap(Sources.withStaticAndNonStaticField).Data.Values.ToList();

            //assert
            Assert.Single(augmentations[0].Fields);
            Assert.DoesNotContain(augmentations[0].Fields, f => f.Name == "b");
        }
    }
}
