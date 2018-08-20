using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using WorkspaceServer.Servers.Roslyn.Instrumentation;
using WorkspaceServer.Tests.Servers.Roslyn.Instrumentation;
using Xunit;

namespace WorkspaceServer.Tests.Instrumentation
{
    public class NewInstrumentationSyntaxRewriterTests
    {
        [Fact]
        public void Rewritten_program_without_instrumentation_is_unchanged()
        {
            var code = Sources.GetDocument(Sources.simple, true);
            var visitor = new InstrumentationSyntaxVisitor(code);
            var rewritten = new InstrumentationSyntaxRewriter(
                visitor.Augmentations.Data.Keys,
                new[] { visitor.VariableLocations },
                new[] { visitor.Augmentations }
                );
            rewritten.ApplyToTree(code.GetSyntaxTreeAsync().Result).GetText()
                .Should().Be(Sources.simple);
        }
        [Fact]
        public void Rewritten_program_with_1_statements_has_1_calls_to_EmitProgramState()
        {
            RewriteCodeWithInstrumentation(@"
using System;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(""Hello World!"");
        }
    }
}"
            ).Should().Be(@"
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(""Hello World!"");
            EmitProgramState(
                GetProgramState(
                    new FilePosition
                    {
                        Line = 10,
                        Character = 1,
                        File = ""Program.cs""
                    }
                )
            );
          
        }
    }
}");
        }
        [Fact]
        public void Rewritten_program_with_2_statements_has_2_calls_to_EmitProgramState()
        {
            RewriteCodeWithInstrumentation(@"
using System;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(""Hello World!"");
            int a = 1;
        }
    }
}"
            ).Should().Be(@"
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(""Hello World!"");
            EmitProgramState(
                GetProgramState(
                    new FilePosition
                    {
                        Line = 10,
                        Character = 1,
                        File = ""Program.cs""
                    }
                )
            );
            int a = 1;
            EmitProgramState(
                GetProgramState(
                    new FilePosition
                    {
                        Line = 10,
                        Character = 1,
                        File = ""Program.cs""
                    },
                    new VariableInfo
                    {
                        Name = nameof(a),
                        Value = JToken.FromObject(a),
                        DeclaredAt = new DeclarationLocation
                        {
                            Start = 0,
                            End = 0
                        }
                    }
                )
            );
        }
    }
}");
        }

        private string RewriteCodeWithInstrumentation(string text)
        {
            var code = Sources.GetDocument(text, true);
            var visitor = new InstrumentationSyntaxVisitor(code);
            var rewritten = new InstrumentationSyntaxRewriter(
                visitor.Augmentations.Data.Keys,
                new[] { visitor.VariableLocations },
                new[] { visitor.Augmentations }
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
