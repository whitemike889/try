using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace WorkspaceServer
{
    public class ScriptingWorkspaceServer : IWorkspaceServer
    {
        public async Task<ProcessResult> CompileAndExecute(BuildAndRunRequest request)
        {
            var scriptOptions = ScriptOptions.Default;

            var referenceAssemblies = new[]
            {
                typeof(object).GetTypeInfo().Assembly,
                typeof(Enumerable).GetTypeInfo().Assembly,
                typeof(Console).GetTypeInfo().Assembly
            };

            scriptOptions = scriptOptions.AddReferences(referenceAssemblies);

            scriptOptions = scriptOptions.AddImports("System");
            scriptOptions = scriptOptions.AddImports("System.Linq");
            scriptOptions = scriptOptions.AddImports("System.Collections.Generic");

            ScriptState<object> state = null;

            using (var console = new RedirectConsoleOutput())
            {
                try
                {
#if false
                    var rawSourceLines = request.RawSource
                                                .Replace("\r\n", "\n")
                                                .Split('\n');

                    foreach (var rawSourceLine in rawSourceLines)
                    {
                        state = await (state?.ContinueWithAsync(rawSourceLine) ??
                                       CSharpScript.RunAsync(
                                           rawSourceLine,
                                           scriptOptions));
                    }
#else
                    state = await
                                CSharpScript.RunAsync(
                                    request.RawSource,
                                    scriptOptions);
#endif

                    Compile(state.Script);
                }
                catch (CompilationErrorException exception)
                {
                    return new ProcessResult(
                        false,
                        exception.Diagnostics.Select(d => d.ToString())
                                 .ToArray());
                }

                return new ProcessResult(
                    succeeded: true,
                    output: console.ToString()
                                   .Replace("\r\n", "\n")
                                   .Split('\n'),
                    returnValue: state?.ReturnValue);
            }
        }

        private static void Compile(Script script)
        {
            var compilation = script.GetCompilation();

            var containsMain = compilation.ContainsSymbolsWithName(s => s == "Main");
            var containsBlah = compilation.ContainsSymbolsWithName(s => s == "blah");

            var entryPoint = compilation.GetEntryPoint(new CancellationToken());

            Console.WriteLine(new { containsMain, containsBlah, entryPoint });

            try
            {
                using (var ms = new MemoryStream())
                {
                    var result = compilation.Emit(ms);

                    if (!result.Success)
                    {
                        var failures = result.Diagnostics
                                             .Where(diagnostic =>
                                                        diagnostic.IsWarningAsError ||
                                                        diagnostic.Severity == DiagnosticSeverity.Error);

                        foreach (Diagnostic diagnostic in failures)
                        {
                            Console.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                        }
                    }
                    else
                    {
                        ms.Seek(0, SeekOrigin.Begin);

                        var assembly = AssemblyLoadContext.Default.LoadFromStream(ms);

                        Console.WriteLine("Successfully emitted in-memory assembly");
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
    }
}
