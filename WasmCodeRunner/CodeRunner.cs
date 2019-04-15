using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace MLS.WasmCodeRunner
{

    public class CodeRunner
    {
        public static InteropMessage<WasmCodeRunnerResponse> ProcessRunRequest(string message)
        {
            var messageObject = JsonConvert.DeserializeObject<InteropMessage<WasmCodeRunnerRequest>>(message);
            if (messageObject.Data.Base64Assembly == null && messageObject.Data.Diagnostics == null)
            {
                // Something was posted that wasn't meant for us
                return null;
            }

            var sequence = messageObject.Sequence;
            var runRequest = messageObject.Data;

            if (runRequest.Succeeded)
            {
                return ExecuteRunRequest(runRequest, sequence);
            }

            var diagnostics = runRequest.Diagnostics.Select(d => d.Message);
            return new InteropMessage<WasmCodeRunnerResponse>(sequence, new WasmCodeRunnerResponse(false, null, diagnostics.ToArray(), null, null));
        }


        public static InteropMessage<WasmCodeRunnerResponse> ExecuteRunRequest(WasmCodeRunnerRequest runRequest, int sequence)
        {
            var output = new List<string>();
            string runnerException = null;
            var bytes = Convert.FromBase64String(runRequest.Base64Assembly);
            var writer = new StringWriter();
            try
            {
                var assembly = Assembly.Load(bytes);

                var currentOut = Console.Out;

                Console.SetOut(writer);

                if (assembly.EntryPoint != null)
                {
                    assembly.EntryPoint.Invoke(null, null);
                }
                else
                {
                    var main = assembly.GetTypes().
                       SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
                       .FirstOrDefault(m => m.Name == "Main");

                    if (main == null)
                    {
                        var result = new WasmCodeRunnerResponse(succeeded: false, exception: null,
                            output: new[] { "error CS5001: Program does not contain a static 'Main' method suitable for an entry point" },
                            diagnostics: Array.Empty<SerializableDiagnostic>(),
                            runnerException: null);

                        return new InteropMessage<WasmCodeRunnerResponse>(sequence, result);
                    }

                    var args = main.GetParameters().Length > 0
                        ? new[] { ExtractCommandLineArgs(runRequest) }
                        : new object[] { };

                    main.Invoke(null, args);
                }
            }
            catch (Exception e)
            {
                if ((e.InnerException ?? e) is TypeLoadException t)
                {
                    runnerException = $"Missing type `{t.TypeName}`";
                }
                if ((e.InnerException ?? e) is MissingMethodException m)
                {
                    runnerException = $"Missing method `{m.Message}`";
                }
                if ((e.InnerException ?? e) is FileNotFoundException f)
                {
                    runnerException = $"Missing file: `{f.FileName}`";
                }

                output.AddRange(SplitOnNewlines(e.ToString()));
            }

            output.AddRange(SplitOnNewlines(writer.ToString()));

            var rb = new WasmCodeRunnerResponse(
                succeeded: true,
                exception: null,
                output: output.ToArray(),
                diagnostics: null,
                runnerException: runnerException);

            return new InteropMessage<WasmCodeRunnerResponse>(sequence, rb);
        }

        private static string[] ExtractCommandLineArgs(WasmCodeRunnerRequest request)
        {
            if (request.RunArgs == null)
            {
                return new string[] { };
            }

            return SplitCommandLine(request.RunArgs);
        }
        private static IEnumerable<string> SplitOnNewlines(string str)
        {
            str = str.Replace("\r\n", "\n");
            return str.Split('\n');
        }

        public static string[] SplitCommandLine(string commandLine)
        {
            var translatedArguments = new StringBuilder(commandLine);
            var escaped = false;
            for (var i = 0; i < translatedArguments.Length; i++)
            {
                if (translatedArguments[i] == '"')
                {
                    escaped = !escaped;
                }
                if (translatedArguments[i] == ' ' && !escaped)
                {
                    translatedArguments[i] = '\n';
                }
            }

            var toReturn = translatedArguments.ToString().Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < toReturn.Length; i++)
            {
                toReturn[i] = RemoveMatchingQuotes(toReturn[i]);
            }
            return toReturn;
        }

        private static string RemoveMatchingQuotes(string stringToTrim)
        {
            var firstQuoteIndex = stringToTrim.IndexOf('"');
            var lastQuoteIndex = stringToTrim.LastIndexOf('"');
            while (firstQuoteIndex != lastQuoteIndex)
            {
                stringToTrim = stringToTrim.Remove(firstQuoteIndex, 1);
                stringToTrim = stringToTrim.Remove(lastQuoteIndex - 1, 1);
                firstQuoteIndex = stringToTrim.IndexOf('"');
                lastQuoteIndex = stringToTrim.LastIndexOf('"');
            }

            return stringToTrim;
        }
    }


}
