using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WorkspaceServer.Servers.Roslyn.Instrumentation
{
    public static class InstrumentedOutputExtractor
    {
        private static readonly string _sentinel = "6a2f74a2-f01d-423d-a40f-726aa7358a81"; //TODO: get this from the syntax re-writer

        public static ProgramOutputStreams ExtractOutput(IReadOnlyCollection<string> outputLines)
        {
            if (outputLines == null || outputLines.Count == 0)
            {
                return new ProgramOutputStreams(outputLines, Array.Empty<string>());
            }

            var newLine = "\n";

            string rawOutput = string.Join(newLine, outputLines);

            var splitOutput = rawOutput
                .TokenizeWithDelimiter(_sentinel)
                .Aggregate(new ExtractorState(), (currentState, nextString) =>
                {
                    if (nextString == _sentinel) return currentState.With(isInstrumentation: !currentState.IsInstrumentation);

                    if (currentState.IsInstrumentation)
                    {
                        // First piece of instrumentation is always program descriptor
                        if (currentState.ProgramDescriptor == "")
                        {
                            return currentState.With(programDescriptor: nextString.Trim());
                        }
                        else
                        {
                            // Why do we need these indices? To figure out how much stdout to expose for
                            // every piece of instrumentation.
                            var (outputStart, outputEnd) = GetCorrespondingStdOutSpan(currentState);

                            var modifiedInstrumentation = (JObject)JsonConvert.DeserializeObject(nextString.Trim());
                            var output = ImmutableSortedDictionary.Create<string, int>()
                                .Add("start", outputStart)
                                .Add("end", outputEnd);
                            var appendedJson = JObject.FromObject(output);
                            modifiedInstrumentation.Add("output", appendedJson);

                            return currentState.With(
                                instrumentation: currentState.Instrumentation.Add(modifiedInstrumentation.ToString())
                            );
                        }
                    }
                    else
                    {
                        var outputStrings = nextString
                            .Trim()
                            .Split(new[] { newLine }, StringSplitOptions.None);
                        return currentState.With(
                            stdOut: currentState.StdOut.Concat(outputStrings).ToImmutableList()
                        );
                    }
                });

            var withStartEnd = AddProgramStartEnd(splitOutput);

            return new ProgramOutputStreams(withStartEnd.StdOut, withStartEnd.Instrumentation, withStartEnd.ProgramDescriptor);
        }

        static (int outputStart, int outputEnd) GetCorrespondingStdOutSpan(ExtractorState currentState)
        {
            if (currentState.StdOut.IsEmpty) return (0, 0);
            else
            {
                var correspondingOutput = currentState.StdOut.Last();
                var endLocation = String.Join("", currentState.StdOut).Length;
                return (endLocation - correspondingOutput.Length, endLocation);
            }
        }

        static IEnumerable<string> TokenizeWithDelimiter(this string input, string delimiter) => Regex.Split(input, $"({delimiter})").Where(str => !String.IsNullOrWhiteSpace(str));

        static ExtractorState AddProgramStartEnd(ExtractorState input)
        {
            var (outputStart, outputEnd) = GetCorrespondingStdOutSpan(input);

            var programStarted = "{ \"stackTrace\": \"Program Started\", \"output\": { \"start\": 0, \"end\": 0 } }";
            var programEnded = "{ \"stackTrace\": \"Program Terminated\", \"output\": { \"start\": " + outputStart + ", \"end\": " + outputEnd + " } }";

            var newInstrumentation = input.Instrumentation.Insert(0, programStarted).Add(programEnded);
            return input.With(instrumentation: newInstrumentation);
        }

    }
}
