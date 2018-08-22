using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using WorkspaceServer.Servers.Roslyn.Instrumentation;
using WorkspaceServer.Tests.Servers.Roslyn.Instrumentation;
using Xunit;

namespace WorkspaceServer.Tests.Instrumentation
{
    public class CodeRewritingTests
    {
        [Fact]
        public void Rewritten_program_without_instrumentation_is_unchanged()
        {
            var code = Sources.GetDocument(Sources.simple, true);
            var visitor = new InstrumentationSyntaxVisitor(code);
            var rewritten = new InstrumentationSyntaxRewriter(
                visitor.Augmentations.Data.Keys,
                visitor.VariableLocations ,
                visitor.Augmentations 
                );
            rewritten.ApplyToTree(code.GetSyntaxTreeAsync().Result).GetText()
                .Should().Be(Sources.simple);
        }
        [Fact]
        public void Rewritten_program_with_1_statements_has_1_calls_to_EmitProgramState()
        {
            var rewrittenCode = RewriteCodeWithInstrumentation(@"
using System;

namespace ConsoleApp2
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine(""Hello World!"");
        }
    }
}"
            );
            string expected = @"
using System;

namespace ConsoleApp2
{
    class Program
    {
        static void Main()
        {
InstrumentationEmitter.EmitProgramState(InstrumentationEmitter.GetProgramState(""{\""line\"":9,\""character\"":12,\""file\"":\""document.cs\""}""));
            Console.WriteLine(""Hello World!"");
        }
    }
}";
            rewrittenCode.ShouldBeEquivalentTo(expected);
        }
        [Fact]
        public void Rewritten_program_with_2_statements_has_2_calls_to_EmitProgramState()
        {
            string actual = RewriteCodeWithInstrumentation(@"
using System;

namespace ConsoleApp2
{
    class Program
    {
        static void Main()
        {
            int a = 1;
            Console.WriteLine(""Hello World!"");
        }
    }
}"
            );
            const string expected = @"
using System;

namespace ConsoleApp2
{
    class Program
    {
        static void Main()
        {
                System.Console.WriteLine(""6a2f74a2-f01d-423d-a40f-726aa7358a81{\""variableLocations\"": [{    \""name\"": \""a\"",    \""locations\"": [{    \""startLine\"": 9,    \""startColumn\"": 16,    \""endLine\"": 9,    \""endColumn\"": 17}],    \""declaredAt\"": {        \""start\"": 117,        \""end\"": 118    }}]}6a2f74a2-f01d-423d-a40f-726aa7358a81"");
                InstrumentationEmitter.EmitProgramState(InstrumentationEmitter.GetProgramState(""{\""line\"":9,\""character\"":12,\""file\"":\""document.cs\""}""));
                int a = 1;
                InstrumentationEmitter.EmitProgramState(InstrumentationEmitter.GetProgramState(""{\""line\"":10,\""character\"":12,\""file\"":\""document.cs\""}"", (""{\""name\"":\""a\"",\""value\"":null,\""declaredAt\"":{\""start\"":117,\""end\"":118}}"", a)));
                Console.WriteLine(""Hello World!"");
        }
    }
}";
            actual.ShouldBeEquivalentTo(expected);
        }

        private string RewriteCodeWithInstrumentation(string text)
        {
            var code = Sources.GetDocument(text, true);
            var visitor = new InstrumentationSyntaxVisitor(code);
            var rewritten = new InstrumentationSyntaxRewriter(
                visitor.Augmentations.Data.Keys,
                 visitor.VariableLocations ,
                 visitor.Augmentations 
                );
            return rewritten.ApplyToTree(code.GetSyntaxTreeAsync().Result).GetText().ToString();

            throw new NotImplementedException();
        }

        [Fact]
        public void Rewritten_program_with_2_statements_has_2_calls_to_GetProgramState() { }

        [Fact]
        public void Rewritten_program_passes_current_file_position_to_GetProgramState() { }

        [Fact]
        public void Rewritten_program_passes_all_in_scope_variables_to_GetProgramState() { }

        [Fact]
        public void Rewritten_program_passes_variable_values_to_GetProgramState() { }

        [Fact]
        public void Rewritten_program_passes_variable_declaredAt_location_to_GetProgramState() { }
    }
}
