using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Recipes;

namespace WorkspaceServer
{
    public class ScriptingWorkspaceServer : IWorkspaceServer
    {
        public async Task<ProcessResult> CompileAndExecute(BuildAndRunRequest request)
        {
            var referenceAssemblies = new[]
            {
                typeof(object).GetTypeInfo().Assembly,
                typeof(Enumerable).GetTypeInfo().Assembly,
                typeof(Console).GetTypeInfo().Assembly
            };

            var options = ScriptOptions.Default
                                       .AddReferences(referenceAssemblies)
                                       .AddImports("System")
                                       .AddImports("System.Linq")
                                       .AddImports("System.Collections.Generic");

            ScriptState<object> state = null;
            var variables = new Dictionary<string, Variable>();

            using (var console = new RedirectConsoleOutput())
            {
                try
                {
#if true
                    var sourceLines = request.RawSource
                                             .Replace("\r\n", "\n")
                                             .Split('\n')
                                             .Select((code, lineNumber) =>
                                                         new { code, lineNumber = lineNumber + 1 })
                                             .ToList();

                    var buffer = new StringBuilder();

                    foreach (var sourceLine in sourceLines)
                    {
                        buffer.AppendLine(sourceLine.code);

                        try
                        {
                            state = await (state?.ContinueWithAsync(buffer.ToString(),
                                                                    catchException: ex => true) ??
                                           CSharpScript.RunAsync(
                                               buffer.ToString(),
                                               options));

                            foreach (var scriptVariable in state.Variables)
                            {
                                variables.GetOrAdd(scriptVariable.Name,
                                                   name => new Variable(name))
                                         .TryAddState(
                                             new VariableState(
                                                 sourceLine.lineNumber,
                                                 scriptVariable.Value,
                                                 scriptVariable.Type));
                            }
                        }
                        catch (CompilationErrorException)
                        {
                            if (sourceLine.lineNumber == sourceLines.Count)
                            {
                                throw;
                            }
                        }
                    }

#else
                    state = await
                                CSharpScript.RunAsync(
                                    request.RawSource,
                                    options);
#endif
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
                    variables: variables.Values,
                    returnValue: state?.ReturnValue);
            }
        }

        private static void Compile(Script script)
        {
            var compilation = script.GetCompilation();

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
                }
            }
        }
    }
}