using FluentAssertions;
using MLS.Agent.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace MLS.Agent.Tests
{
    public class InstrumentationExtractorTests
    {
        private static string Sentinel = "6a2f74a2-f01d-423d-a40f-726aa7358a81";
        private readonly List<String> programOutput = new List<String>()
            {
                Sentinel,
                "instrumentation goes here",
                Sentinel,
                "program output",
                Sentinel,
                "more instrumentation",
                Sentinel,
                "even more output"
            };
        private ProgramOutputStreams splitOutput;

        public InstrumentationExtractorTests()
        {
            splitOutput = InstrumentedOutputExtractor.ExtractOutput(programOutput, "\n");
        }

        [Fact]
        public void Extracted_Output_Should_Have_Correct_First_Line()
        {
            splitOutput.StdOut.First().Should().Be("program output");
        }

        [Fact]
        public void Extracted_Output_Should_Have_Correct_Second_Line()
        {
            splitOutput.StdOut.ElementAt(1).Should().Be("even more output");
        }

        [Fact]
        public void Extracted_Instrumentation_Should_Have_Correct_Second_Line()
        {
            splitOutput.Instrumentation.ElementAt(2).Should().Be("more instrumentation \"output\": { \"start\":0, \"end\":15}}");
        }

        [Fact]
        public void Extracted_Instrumentation_Should_Have_Correct_First_Line()
        {
            splitOutput.Instrumentation.ElementAt(1).Should().Be("instrumentation goes here \"output\": { \"start\":0, \"end\":0}}");
        }
        
        [Fact]
        public void Extracted_Instrumentation_Should_Have_Program_Start()
        {
            splitOutput.Instrumentation.First().Should().Be("{ stackTrace: \"Program Started\", output: { start: 0, end: 0 } }");
        }
        [Fact]
        public void Extracted_Instrumentation_Should_Have_Program_End()
        {
            splitOutput.Instrumentation.Last().Should().Contain("{ stackTrace: \"Program Terminated\", output: { start: 15, end: 31 } }");
        }

    }
}
