using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Clockwise;
using Microsoft.CodeAnalysis;
using Diagnostic = Microsoft.CodeAnalysis.Diagnostic;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Recommendations;
using Microsoft.CodeAnalysis.Scripting;
using Pocket;
using WorkspaceServer.Models;
using WorkspaceServer.Models.Completion;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Models.SingatureHelp;
using WorkspaceServer.Transformations;
using static Pocket.Logger<WorkspaceServer.Servers.Scripting.ScriptingWorkspaceServer>;
using Workspace = WorkspaceServer.Models.Execution.Workspace;
using WorkspaceServer.Servers.Roslyn;

namespace WorkspaceServer.Servers.Scripting
{
    public class ScriptingWorkspaceServer : ICodeRunner, ILanguageService
    {
        private readonly BufferInliningTransformer _transformer = new BufferInliningTransformer();
        private readonly WorkspaceFixture _fixture;

        public ScriptingWorkspaceServer()
        {
            _fixture = new WorkspaceFixture(
                WorkspaceUtilities.DefaultUsings,
                WorkspaceUtilities.DefaultReferencedAssemblies);
        }
        public async Task<RunResult> Run(Workspace workspace, Budget budget = null)
        {
            budget = budget ?? new Budget();

            using (var operation = Log.OnEnterAndConfirmOnExit())
            using (var console = await ConsoleOutput.Capture(budget))
            {
                workspace = await _transformer.TransformAsync(workspace, budget);

                if (workspace.Files.Count != 1)
                {
                    throw new ArgumentException($"{nameof(workspace)} should have exactly one source file.");
                }

                var options = CreateOptions(workspace);

                ScriptState<object> state = null;
                Exception userException = null;

                var buffer = new StringBuilder(workspace.GetSourceFiles().Single().Text.ToString());

                try
                {
                    state = await Run(buffer, options, budget);

                    if (console.IsEmpty())
                    {
                        state = await EmulateConsoleMainInvocation(state, buffer, options, budget);
                    }

                    budget.RecordEntry(UserCodeCompletedBudgetEntryName);
                }
                catch (CompilationErrorException ex)
                {
                    userException = ex;

                    Console.WriteLine(
                        String.Join(Environment.NewLine,
                                    ex.Diagnostics
                                      .Select(d => d.ToString())));
                }
                catch (Exception ex)
                {
                    userException = ex;
                }

                budget.RecordEntryAndThrowIfBudgetExceeded();

                var processeddiagnostics = await GetDiagnostics(workspace, options);
                var diagnostics = processeddiagnostics.Select(e => e.Diagnostic).ToArray();
                var output = console.StandardOutput
                                    .Replace("\r\n", "\n")
                                    .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                output = ProcessOutputLines(output,
                                            processeddiagnostics
                                                .Where(e => e.Diagnostic.Severity == DiagnosticSeverity.Error)
                                                .Select(e => e.ErrorMessage)
                                                .ToArray());
                var result = new RunResult(
                    succeeded: !userException.IsConsideredRunFailure(),
                    output: output,
                    exception: (userException ?? state?.Exception).ToDisplayString(),
                    diagnostics: diagnostics);

                operation.Complete(result, budget);

                return result;
            }
        }

        private string[] ProcessOutputLines(string[] output, string[] errormessages)
        {
            var filter = output.Where(IsNotDisagnostic);

            return filter.Concat(errormessages).ToArray();
        }


        private bool IsNotDisagnostic(string line)
        {
            var filter = new Regex(@"^(?<location>\(\d+,\d+\):)\s*(?<level>\S+)\s*(?<code>[A-Z]{2}\d+:)(?<message>.+)", RegexOptions.Compiled);
            return !filter.IsMatch(line);
        }

        private static ScriptOptions CreateOptions(Workspace request) =>
            ScriptOptions.Default
                         .AddReferences(GetReferenceAssemblies())
                         .AddImports(WorkspaceUtilities.DefaultUsings.Concat(request.Usings));

        private async Task<(SerializableDiagnostic Diagnostic, string ErrorMessage)[]> GetDiagnostics(
            Workspace workspace,
            ScriptOptions options, Budget budget = null)
        {
            budget = budget ?? new Budget();

            var processor = new BufferInliningTransformer();
            var processed = await processor.TransformAsync(workspace, budget);
            var viewPorts = processor.ExtractViewPorts(processed);
            var sourceFile = processed.GetSourceFiles().Single();
            var code = sourceFile.Text.ToString();
            return await ServiceHelpers.GetProjectedDiagnostics(workspace, CSharpScript.Create(code, options).GetCompilation());
        }

        private static Task<ScriptState<object>> Run(
            StringBuilder buffer,
            ScriptOptions options,
            Budget budget) =>
            Task.Run(() =>
                         CSharpScript.RunAsync(
                             buffer.ToString(),
                             options))
                .CancelIfExceeds(budget, () => null);

        private static Assembly[] GetReferenceAssemblies() =>
            new[]
            {
                typeof(object).GetTypeInfo().Assembly,
                typeof(Enumerable).GetTypeInfo().Assembly,
                typeof(Console).GetTypeInfo().Assembly
            };

        public async Task<CompletionResult> GetCompletionList(WorkspaceRequest request, Budget budget = null)
        {
            budget = budget ?? new Budget();
            using (Log.OnExit())
            {
                var (document, position) = await GenerateDocumentAndPosition(request, budget);
                var service = CompletionService.GetService(document);

                var completionList = await service.GetCompletionsAsync(document, position);
                var semanticModel = await document.GetSemanticModelAsync();
                var symbols = await Recommender.GetRecommendedSymbolsAtPositionAsync(semanticModel, request.Position, document.Project.Solution.Workspace);

                var symbolToSymbolKey = new Dictionary<(string, int), ISymbol>();
                foreach (var symbol in symbols)
                {
                    var key = (symbol.Name, (int) symbol.Kind);
                    if (!symbolToSymbolKey.ContainsKey(key))
                    {
                        symbolToSymbolKey[key] = symbol;
                    }
                }

                var items = completionList.Items.Select(item => item.ToModel(symbolToSymbolKey, document));

                return new CompletionResult(items: await Task.WhenAll(items));
            }
        }

        public async Task<SignatureHelpResponse> GetSignatureHelp(WorkspaceRequest request, Budget budget = null)
        {
            budget = budget ?? new Budget();
            using (Log.OnExit())
            {
                var (document, position) = await GenerateDocumentAndPosition(request, budget);
                var response = await SignatureHelpService.GetSignatureHelp(document, position, budget);
                return response;
            }
        }

        private async Task<(Document document, int position)> GenerateDocumentAndPosition(WorkspaceRequest request, Budget budget)
        {
            var processor = new BufferInliningTransformer();
            var workspace = await processor.TransformAsync(request.Workspace, budget);

            if (workspace.Files.Count != 1)
            {
                throw new ArgumentException($"{nameof(request)} should have exactly one source file.");
            }

            var code = workspace.Files.Single().Text;
            var position = workspace.Buffers.First(b => b.Id == request.ActiveBufferId).Position + request.Position;

            var document = _fixture.ForkDocument(code);
            return (document, position);
        }

        private static async Task<ScriptState<object>> EmulateConsoleMainInvocation(
            ScriptState<object> state,
            StringBuilder buffer,
            ScriptOptions options,
            Budget budget)
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

                state = await Run(buffer, options, budget);
            }

            return state;

            IMethodSymbol EntryPointType() =>
                EntryPointFinder.FindEntryPoint(
                    script.GetCompilation().GlobalNamespace);

            string ParametersForMain() => entryPointMethod.Parameters.Any()
                                              ? "new object[]{ new string[0] }"
                                              : "null";
        }

        public async Task<DiagnosticResult> GetDiagnostics(Workspace request, Budget budget = null) =>
            new DiagnosticResult((await GetDiagnostics(request, CreateOptions(request), budget)).Select(e => e.Diagnostic).ToArray());

        public static string UserCodeCompletedBudgetEntryName = "UserCodeCompleted";
    }
}
