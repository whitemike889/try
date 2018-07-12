using FluentAssertions;
using FluentAssertions.Primitives;
using MLS.Agent.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using WorkspaceServer.Servers.Roslyn.Instrumentation;
using WorkspaceServer.Tests;
using Xunit;

namespace WorkspaceServer.Tests.Servers.Roslyn.Instrumentation
{
    public class InstrumentedOutputExtractorTests
    {
        private static string _sentinel = "6a2f74a2-f01d-423d-a40f-726aa7358a81";
        private readonly List<String> instrumentedProgramOutput = new List<String>()
            {
                _sentinel,
            #region variableLocation
            @"
{
""variableLocations"": [
    {
        ""name"": ""b"",
        ""locations"": [
          {
            ""startLine"": 12,
            ""startColumn"": 16,
            ""endLine"": 12,
            ""endColumn"": 21
          }
        ],
        ""declaredAt"": {
          ""start"": 176,
          ""end"": 181
        }
    }
]
}",
            #endregion
                _sentinel + _sentinel,
            #region programState
            @"
{
      ""filePosition"": {
        ""line"": 12,
        ""character"": 12,
        ""file"": ""Program.cs""
      },
      ""stackTrace"": ""    at FibonacciTest.Program.Main()\r\n "",
      ""locals"": [
        {
          ""name"": ""a"",
          ""value"": ""4"",
          ""declaredAt"": {
            ""start"": 153,
            ""end"": 154
          }
        }
      ],
      ""parameters"": [],
      ""fields"": []
}
",
#endregion
                _sentinel,
                "program output",
                _sentinel,
            #region programState
            @"
{
      ""filePosition"": {
        ""line"": 13,
        ""character"": 12,
        ""file"": ""Program.cs""
      },
      ""stackTrace"": ""    at FibonacciTest.Program.Main()\r\n "",
      ""locals"": [],
      ""parameters"": [{
          ""name"": ""p"",
          ""value"": ""1"",
          ""declaredAt"": {
            ""start"": 1,
            ""end"": 1
          }
        }],
      ""fields"": [{
          ""name"": ""f"",
          ""value"": ""2"",
          ""declaredAt"": {
            ""start"": 2,
            ""end"": 2
          }
        }]
}
",
#endregion
                _sentinel,
                "even more output"
            };

        private ProgramOutputStreams splitOutput;

        public InstrumentedOutputExtractorTests()
        {
            var normalizedOutput = instrumentedProgramOutput.Select(line => line.EnforceLF()).ToArray();
            splitOutput = InstrumentedOutputExtractor.ExtractOutput(normalizedOutput);
        }

        public class Non_Sentinel_Bounded_Strings_Are_Parsed_As_Output : InstrumentedOutputExtractorTests
        {
            [Fact]
            public void It_Should_Have_Correct_First_Line()
            {
                splitOutput.StdOut.First().Should().Be("program output");
            }

            [Fact]
            public void It_Should_Have_Correct_Second_Line()
            {
                splitOutput.StdOut.ElementAt(1).Should().Be("even more output");
            }
        }

        public class First_Sentinel_Bounded_String_Is_Parsed_As_ProgramDescriptor : InstrumentedOutputExtractorTests
        {
            [Fact]
            public void It_Should_Have_Correct_Variable_Name()
            {
                splitOutput.ProgramDescriptor.VariableLocations.First().Name.Should().Be("b");
            }

            [Fact]
            public void It_Should_Have_Correct_Location()
            {
                splitOutput.ProgramDescriptor.VariableLocations.First().Locations.First().StartColumn.Should().Be(16);
            }
        }

        public class Rest_Of_Sentinel_Bounded_Strings_Are_Parsed_As_ProgramState : InstrumentedOutputExtractorTests
        {
            [Fact]
            public void First_Program_State_Has_Correct_Local_Name()
            {
                splitOutput.ProgramStatesArray.ProgramStates.ElementAt(1).Locals.First().Name.Should().Be("a");
            }

            [Fact]
            public void Second_Program_State_Has_Correct_Parameter_Name()
            {
                splitOutput.ProgramStatesArray.ProgramStates.ElementAt(2).Parameters.First().Name.Should().Be("p");
            }

            [Fact]
            public void Second_Program_State_Has_Correct_Field_Name()
            {
                splitOutput.ProgramStatesArray.ProgramStates.ElementAt(2).Fields.First().Name.Should().Be("f");
            }

            [Fact]
            public void Dummy_Program_Start_Should_Not_Have_Variables()
            {
                splitOutput.ProgramStatesArray.ProgramStates.First().Locals.Should().BeNull();
            }

        }

    }
}


