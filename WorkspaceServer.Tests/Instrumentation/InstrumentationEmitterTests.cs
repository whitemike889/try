using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Recipes;
using WorkspaceServer.Servers.Roslyn;
using WorkspaceServer.Servers.Roslyn.Instrumentation;
using WorkspaceServer.Servers.Roslyn.Instrumentation.Contract;
using Xunit;

namespace WorkspaceServer.Tests.Instrumentation
{
    public class InstrumentationEmitterTests
    {

        private JToken programStateJson;
        private string programStateString;
        public InstrumentationEmitterTests()
        {
            var a = 1;
            var b = "two";
            var c = a;

            programStateJson = InstrumentationEmitter.GetProgramState(
                    new FilePosition
                    {
                        Line = 1,
                        Character = 2,
                        File = "test.cs"
                    }.ToJson(),
                    (new VariableInfo
                    {
                        Name = nameof(a),
                        Value = JToken.FromObject(a),
                        DeclaredAt = new DeclarationLocation
                        {
                            Start = 10,
                            End = 11
                        }
                    }.ToJson(),
                    a
                    ),
                    (new VariableInfo
                    {
                        Name = nameof(b),
                        Value = JToken.FromObject(b),
                        DeclaredAt = new DeclarationLocation
                        {
                            Start = 20,
                            End = 21
                        }
                    }.ToJson(),
                    b)
                );
            InstrumentationEmitter.EmitProgramState(programStateJson);
            programStateString = programStateJson.ToString();
        }

        private ProgramStateAtPosition getJson()
        {
            return JsonConvert.DeserializeObject<ProgramStateAtPosition>(programStateString);
        }
        [Fact]
        public async Task It_Emits_Right_Format_With_Sentinels_Around_JSONAsync()
        {
            using (var output = await ConsoleOutput.Capture())
            {
                InstrumentationEmitter.EmitProgramState(programStateJson);
                output.StandardOutput.Should().Match(InstrumentationEmitter.Sentinel + "*" + InstrumentationEmitter.Sentinel);
            }
        }

        [Fact]
        public void It_Emits_JSON_That_Can_Be_Deserialized_Into_Existing_Model()
        {
            getJson().Should().NotBeNull();
        }

        [Fact]
        public void Emitted_JSON_Has_Correct_Variable_Value_For_Int()
        {
            getJson().Locals.Where(v => v.Name == "a").First().Value.Should().Equals(1);
        }

        [Fact]
        public void Emitted_Json_Has_Correct_DeclaredAt_Start_For_A()
        {
            getJson().Locals.Where(v => v.Name == "a").First().DeclaredAt.Start.Should().Equals(10);
        }

        [Fact]
        public void Emitted_Json_Has_Correct_DeclaredAt_End_For_A()
        {
            getJson().Locals.Where(v => v.Name == "a").First().DeclaredAt.End.Should().Equals(11);
        }

        [Fact]
        public void Emitted_JSON_Has_Correct_Variable_Value_For_String()
        {
            getJson().Locals.Where(v => v.Name == "b").First().Value.ToString().Should().Be("two");
        }

        [Fact]
        public void Emitted_JSON_Has_File_Position_Character()
        {
            getJson().FilePosition.Character.Should().Be(2);
        }

        [Fact]
        public void Emitted_JSON_Has_File_Position_Line()
        {
            getJson().FilePosition.Line.Should().Be(1);
        }
    }
}
