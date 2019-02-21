using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace MLS.WasmCodeRunner
{

    public class CodeRunner
    {
        public static InteropMessage<RunResponse> ProcessCompileResult(string message)
        {
            var messageObject = JsonConvert.DeserializeObject<InteropMessage<CompileResult>>(message);
            if (messageObject.data.base64assembly == null && messageObject.data.diagnostics == null)
            {
                // Something was posted that wasn't meant for us
                return null;
            }

            int sequence = messageObject.sequence;
            var compileResult = messageObject.data;

            if (compileResult.succeeded)
            {
                return ProcessCompileResult(compileResult, sequence);
            }
            else
            {
                var diagnostics = compileResult.diagnostics.Select(d => d.Message);
                return new InteropMessage<RunResponse>(sequence, new RunResponse(false, null, diagnostics.ToArray(), null, null));
            }
        }


        public static InteropMessage<RunResponse> ProcessCompileResult(CompileResult compileResult, int sequence)
        {
            List<string> output = new List<string>();
            string runnerException = null;
            var bytes = System.Convert.FromBase64String(compileResult.base64assembly);
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
                        var result = new RunResponse(succeeded: false, exception: null,
                            output: new[] { "error CS5001: Program does not contain a static 'Main' method suitable for an entry point" },
                            diagnostics: Array.Empty<SerializableDiagnostic>(),
                            runnerException: null);

                        return new InteropMessage<RunResponse>(sequence, result);
                    }

                    var args = main.GetParameters().Length > 0
                        ? new[] { new string[] { } }
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

            var rb = new RunResponse(
                succeeded: true,
                exception: null,
                output: output.ToArray(),
                diagnostics: null,
                runnerException: runnerException);

            return new InteropMessage<RunResponse>(sequence, rb);
        }

        private static string[] SplitOnNewlines(string str)
        {
            str = str.Replace("\r\n", "\n");
            return str.Split('\n');
        }
    }
}
