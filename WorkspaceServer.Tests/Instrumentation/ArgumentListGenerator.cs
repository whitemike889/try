using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Recipes;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace WorkspaceServer.Tests.Instrumentation
{
    public class ArgumentListGenerator
    {
        [Fact]
        public void It_can_pass_through_an_argument()
        {
            var argument = new { foo = 3 };

            var list = GenerateArgumentListForGetProgramState(argument);

            var text = list.ToString();
            var expected = "(\"{\\\"foo\\\":3}\")";
            Assert.Equal(expected, text);
        }

        [Fact]
        public void It_can_pass_through_2_arguments()
        {
            var argument = new { foo = 3 };
            var secondArgument = new { bar = 2 };

            var list = GenerateArgumentListForGetProgramState(argument, secondArgument);

            var text = list.ToString();
            var expected = "(\"{\\\"foo\\\":3}\",\"{\\\"bar\\\":2}\")";
            Assert.Equal(expected, text);
        }

        private ArgumentListSyntax GenerateArgumentListForGetProgramState(params object[] argumentList)
        {
            var argumentArray = argumentList.Select(argument =>
            {
                return SyntaxFactory.Argument(
                    SyntaxFactory.LiteralExpression(
                        SyntaxKind.StringLiteralExpression,
                        SyntaxFactory.Literal(argument.ToJson())
                    )
                );
            }).ToArray();

            return SyntaxFactory.ArgumentList(
                SyntaxFactory.SeparatedList<ArgumentSyntax>(argumentArray)
            );
        }
    }
}
