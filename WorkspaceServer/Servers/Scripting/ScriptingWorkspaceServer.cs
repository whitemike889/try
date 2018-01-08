using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Scripting;
using Pocket;
using Recipes;
using WorkspaceServer.Models.Completion;
using WorkspaceServer.Models.Execution;
using static Pocket.Logger<WorkspaceServer.Servers.Scripting.ScriptingWorkspaceServer>;

namespace WorkspaceServer.Servers.Scripting
{
    public class ScriptingWorkspaceServer : IWorkspaceServer
    {
#if DEBUG
        public const int DefaultTimeoutInSeconds = 10;
#else
        public const int DefaultTimeoutInSeconds = 5;
#endif

        private readonly TimeSpan _defaultTimeout;

        public ScriptingWorkspaceServer(int defaultTimeoutInSeconds = DefaultTimeoutInSeconds)
        {
            if (defaultTimeoutInSeconds < 1)
            {
                throw new ArgumentException($"{nameof(defaultTimeoutInSeconds)} must be at least 1.");
            }

            _defaultTimeout = TimeSpan.FromSeconds(defaultTimeoutInSeconds);
        }

        public async Task<RunResult> Run(RunRequest request, TimeSpan? timeout = null)
        {
            using (Log.OnEnterAndExit())
            using (var console = await ConsoleOutput.Capture())
            {

                var options = ScriptOptions.Default
                                           .AddReferences(GetReferenceAssemblies())
                                           .AddImports(GetDefultUsings().Concat(request.Usings));

                ScriptState<object> state = null;
                var variables = new Dictionary<string, Variable>();
                Exception exception = null;

                try
                {
                    await Task.Run(async () =>
                    {
                        var sourceLines = SourceFile.Create(request.RawSource).Text.Lines;

                        var buffer = new StringBuilder();

                        for (var index = 0; index < sourceLines.Count; index++)
                        {
                            var sourceLine = sourceLines[index];
                            buffer.AppendLine(sourceLine.ToString());

                            // Convert from 0-based to 1-based.
                            var lineNumber = sourceLine.LineNumber + 1;

                            try
                            {
                                console.Clear();

                                state = await Run(state, buffer, options);

                                CaptureVariableState(state, variables, lineNumber);

                                if (index == sourceLines.Count - 1 &&
                                    console.IsEmpty())
                                {
                                    state = await EmulateConsoleMainInvocation(state, buffer, options);
                                }
                            }
                            catch (CompilationErrorException ex)
                            {
                                if (lineNumber == sourceLines.Count)
                                {
                                    exception = ex;

                                    Console.WriteLine(
                                        string.Join(Environment.NewLine,
                                                    ex.Diagnostics
                                                      .Select(d => d.ToString())));

                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                exception = ex;
                                break;
                            }
                        }
                    }).Timeout(timeout ?? _defaultTimeout);
                }
                catch (TimeoutException timeoutException)
                {
                    exception = timeoutException;
                }

                return new RunResult(
                    succeeded: !(exception is TimeoutException) &&
                               !(exception is CompilationErrorException),
                    output: console.StandardOutput
                                   .Replace("\r\n", "\n")
                                   .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries),
                    returnValue: state?.ReturnValue,
                    exception: ToDisplayString(exception ?? state?.Exception),
                    variables: variables.Values);
            }
        }

        private static void CaptureVariableState(ScriptState<object> state, Dictionary<string, Variable> variables, int lineNumber)
        {
            foreach (var scriptVariable in state.Variables)
            {
                variables.GetOrAdd(scriptVariable.Name,
                                   name => new Variable(name))
                         .TryAddState(
                             new VariableState(
                                 lineNumber,
                                 scriptVariable.Value,
                                 scriptVariable.Type));
            }
        }

        internal static string ToDisplayString(Exception exception)
        {
            switch (exception)
            {
                case CompilationErrorException _:
                    return null;

                default:
                    return exception?.ToString();
            }
        }

        private static async Task<ScriptState<object>> Run(
            ScriptState<object> state,
            StringBuilder buffer,
            ScriptOptions options) =>
            state == null
                ? await CSharpScript.RunAsync(
                      buffer.ToString(),
                      options)
                : await state.ContinueWithAsync(
                      buffer.ToString(),
                      catchException: ex => false);

        private static Assembly[] GetReferenceAssemblies() =>
            new[]
            {
                typeof(object).GetTypeInfo().Assembly,
                typeof(Enumerable).GetTypeInfo().Assembly,
                typeof(Console).GetTypeInfo().Assembly
            };

        private static string[] GetDefultUsings() =>
            new[] { "System", "System.Linq", "System.Collections.Generic" };

        public async Task<CompletionResult> GetCompletionList(CompletionRequest request)
        {
            using (Log.OnExit())
            {
                var sourceFile = SourceFile.Create(request.RawSource, request.Position);

                var document = CreateDocument(sourceFile);
                var service = CompletionService.GetService(document);

                var completionList = await service.GetCompletionsAsync(
                                         document,
                                         request.Position);

                return new CompletionResult(
                    items: completionList.Items.Select(item => item.ToModel()).ToArray());
            }
        }

        private static Document CreateDocument(SourceFile sourceFile)
        {
            var workspace = new AdhocWorkspace(MefHostServices.DefaultHost);

            var projectId = ProjectId.CreateNewId("ScriptProject");

            var metadataReferences = GetReferenceAssemblies()
                .Select(a => MetadataReference.CreateFromFile(a.Location));

            var compilationOptions = new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                usings: GetDefultUsings());

            var projectInfo = ProjectInfo.Create(
                projectId,
                version: VersionStamp.Create(),
                name: "ScriptProject",
                assemblyName: "ScriptProject",
                language: LanguageNames.CSharp,
                compilationOptions: compilationOptions,
                metadataReferences: metadataReferences);

            workspace.AddProject(projectInfo);

            var documentId = DocumentId.CreateNewId(projectId, "ScriptDocument");

            var documentInfo = DocumentInfo.Create(documentId,
                                                   name: "ScriptDocument",
                                                   sourceCodeKind: SourceCodeKind.Script);

            workspace.AddDocument(documentInfo);

            var solution = workspace.CurrentSolution
                                    .WithDocumentText(documentId, sourceFile.Text);

            workspace.TryApplyChanges(solution);

            return workspace.CurrentSolution.GetDocument(documentId);
        }

        private static async Task<ScriptState<object>> EmulateConsoleMainInvocation(
            ScriptState<object> state,
            StringBuilder buffer,
            ScriptOptions options)
        {
            var script = state.Script;
            var compiled = script.Compile();

            if (compiled.FirstOrDefault(d => d.Descriptor.Id == "CS7022")
                    is Diagnostic noEntryPointWarning &&
                EntryPointType()
                    is IMethodSymbol entryPointMethod)
            {
                // e.g. warning CS7022: The entry point of the program is global script code; ignoring 'Program.Main()' entry point.

                // add a line of code to call Main using reflection
                buffer.AppendLine(
                    $@"
typeof({entryPointMethod.ContainingType.Name})
    .GetMethod(""Main"",
               System.Reflection.BindingFlags.Static |
               System.Reflection.BindingFlags.NonPublic |
               System.Reflection.BindingFlags.Public)
    .Invoke(null, {ParametersForMain()});");

                state = await Run(state, buffer, options);
            }

            return state;

            IMethodSymbol EntryPointType() =>
                EntryPointFinder.FindEntryPoint(
                    script.GetCompilation().GlobalNamespace);

            string ParametersForMain() => entryPointMethod.Parameters.Any()
                                              ? "new object[]{ new string[0] }"
                                              : "null";
        }
    }
}
