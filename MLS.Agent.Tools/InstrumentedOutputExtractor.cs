using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MLS.Agent.Tools
{
    public static class InstrumentedOutputExtractor
    {
        private static readonly string sentinel = "6a2f74a2-f01d-423d-a40f-726aa7358a81"; //TODO: get this from the syntax re-writer

        public static ProgramOutputStreams ExtractOutput(IReadOnlyCollection<string> output, string newLine = null)
        {
            if (output == null || output.Count == 0)
            {
                return new ProgramOutputStreams(output, Array.Empty<string>());
            }

            newLine = newLine ?? Environment.NewLine;

            // extractor works on a single block of text at the moment, so re-join the emitted lines into a single block
            string rawOutput = string.Join(newLine, output);

            StringBuilder stdOut = new StringBuilder();
            var instrumentationOut = new List<string>() { GetDummyOutput("Program Started", 0, 0) };

            var currentSentinelIndex = -1;
            var lastSentinelIndex = 0;
            var lastOutputIndex = 0;
            while ((currentSentinelIndex = rawOutput.IndexOf(sentinel, lastSentinelIndex)) != -1)
            {
                //everything up to index must be real output
                var realOutput = rawOutput.Substring(lastSentinelIndex, currentSentinelIndex - lastSentinelIndex);
                lastOutputIndex += realOutput.Length;
                stdOut.Append(realOutput);

                // move past the first sentinal
                lastSentinelIndex = currentSentinelIndex + sentinel.Length;
                if (lastSentinelIndex > rawOutput.Length)
                {
                    // handle the case that the output wasn't totally collected
                    lastSentinelIndex = rawOutput.Length;
                    break;
                }

                // find the end of the diagnostic output
                var nextSentinelIndex = rawOutput.IndexOf(sentinel, lastSentinelIndex);
                if (nextSentinelIndex == -1)
                {
                    lastSentinelIndex = rawOutput.Length;
                    break;
                }

                // get the instrumentation, and append the real output indicies as a json field to the step
                int length = nextSentinelIndex - lastSentinelIndex - 1;
                var instrumentationStep = rawOutput
                    .Substring(lastSentinelIndex, length)
                    .Trim();
                instrumentationStep += " \"output\": { \"start\":" + (lastOutputIndex - realOutput.Length) + ", \"end\":" + lastOutputIndex + "}}";
                instrumentationOut.Add(instrumentationStep);

                // move past this step to look for the next one
                lastSentinelIndex = nextSentinelIndex + sentinel.Length + newLine.Length;
                if (lastSentinelIndex > rawOutput.Length)
                {
                    lastSentinelIndex = rawOutput.Length;
                    break;
                }
            }

            // anything left is normal output
            stdOut.Append(rawOutput.Substring(lastSentinelIndex, rawOutput.Length - lastSentinelIndex));

            // end the instrumentation
            instrumentationOut.Add(GetDummyOutput("Program Terminated", stdOut.Length - (rawOutput.Length - lastSentinelIndex), stdOut.Length));

            // split the output up by new-line, so its back in the format the caller expects it in
            var splitOut = stdOut.ToString().Split(new string[] { newLine }, StringSplitOptions.None);

            return new ProgramOutputStreams(splitOut, instrumentationOut);
        }

        private static string GetDummyOutput(string trace, int outputPos, int outputEnd)
        {
            return $"{{ stackTrace: \"{trace}\", output: {{ start: {outputPos}, end: {outputEnd} }} }}";
        }
    }
}
