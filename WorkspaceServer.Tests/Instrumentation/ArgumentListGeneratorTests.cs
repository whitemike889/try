using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Recipes;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Xunit;
using WorkspaceServer.Servers.Roslyn.Instrumentation;
using WorkspaceServer.Servers.Roslyn.Instrumentation.Contract;
using Newtonsoft.Json.Linq;
using FluentAssertions;

namespace WorkspaceServer.Tests.Instrumentation
{
    public class ArgumentListGeneratorTests
    {
        

        [Fact]
        public void CreateSyntaxNode_works_correctly()
        {
            var a = new { foo = 3 };
            var vi = new VariableInfo
            {
                Name = nameof(a),
                Value = JToken.FromObject(a),
                DeclaredAt = new DeclarationLocation
                {
                    Start = 10,
                    End = 11
                }
            };
            var filePosition = new FilePosition
            {
                Line = 1,
                Character = 1,
                File = "test.cs"
            };

            var result = InstrumentationSyntaxRewriter.CreateSyntaxNode(filePosition, vi);
            result.ToString().Should().Be(@"InstrumentationEmitter.EmitProgramState((""{\""name\"":\""a\"",\""value\"":{\""foo\"":3},\""declaredAt\"":{\""start\"":10,\""end\"":11}}"",a));");
        }

        [Fact]
        public void It_can_pass_through_an_argument()
        {
            var argument = new { foo = 3 };

            var list = ArgumentListGenerator.GenerateArgumentListForGetProgramState(new FilePosition
            {
                Line = 1,
                Character = 1,
                File = "test.cs"
            },
            (argument, "foo"));

            var text = list.ToString();
            var expected = "((\"{\\\"foo\\\":3}\",foo))";
            Assert.Equal(expected, text);
        }

        [Fact]
        public void It_can_pass_through_2_arguments()
        {
            var argument = new { foo = 3 };
            var secondArgument = new { bar = 2 };

            var list = ArgumentListGenerator.GenerateArgumentListForGetProgramState(new FilePosition
            {
                Line = 1,
                Character = 1,
                File = "test.cs"
            },(argument, "foo"), (secondArgument, "bar"));

            var text = list.ToString();
            var expected = "((\"{\\\"foo\\\":3}\",foo),(\"{\\\"bar\\\":2}\",bar))";
            Assert.Equal(expected, text);
        }

    }
}
