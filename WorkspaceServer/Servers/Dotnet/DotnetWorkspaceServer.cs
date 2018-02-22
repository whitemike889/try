using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Clockwise;
using Microsoft.CodeAnalysis;
using MLS.Agent.Tools;
using Pocket;
using WorkspaceServer.Models.Completion;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Transformations;
using Diagnostic = OmniSharp.Client.Diagnostic;
using static Pocket.Logger<WorkspaceServer.Servers.Dotnet.DotnetWorkspaceServer>;
using Workspace = MLS.Agent.Tools.Workspace;
using OmnisharpEmitResponse = OmniSharp.Client.Commands.OmniSharpResponseMessage<OmniSharp.Client.Commands.EmitResponse>;
using TextSpan = Microsoft.CodeAnalysis.Text.TextSpan;

namespace WorkspaceServer.Servers.Dotnet
{
    public class DotnetWorkspaceServer : IWorkspaceServer, IDisposable
    {
        private readonly Workspace _workspace;
        private readonly OmniSharpServer _omniSharpServer;
        private readonly AsyncLazy<bool> _initialized;
        private bool _disposed;
        private readonly Budget _initializationBudget = new Budget();
        private readonly TimeSpan _defaultTimeoutInSeconds;
        public static string UserCodeCompletedBudgetEntryName = "UserCodeCompleted";

        public DotnetWorkspaceServer(
            Workspace workspace,
            int? defaultTimeoutInSeconds = 30)
        {
            _workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));

            _defaultTimeoutInSeconds = TimeSpan.FromSeconds(defaultTimeoutInSeconds ?? 30);

#if DEBUG
            var logToPocketLogger = true;
#else
            var logToPocketLogger = false;
#endif

            _omniSharpServer = new OmniSharpServer(
                _workspace.Directory,
                Paths.EmitPlugin,
                logToPocketLogger: logToPocketLogger);

            _initialized = new AsyncLazy<bool>(async () =>
            {
                await _workspace.EnsureBuilt(_initializationBudget);
                await _omniSharpServer.WorkspaceReady(_initializationBudget);
                return true;
            });
        }

        public async Task EnsureInitializedAndNotDisposed(Budget budget = null)
        {
            budget?.RecordEntryAndThrowIfBudgetExceeded();

            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(DotnetWorkspaceServer));
            }

            await _initialized.ValueAsync()
                              .CancelIfExceeds(budget ?? new Budget());
        }

        public async Task<RunResult> Run(Models.Execution.Workspace request, Budget budget = null)
        {
            budget = budget ?? new TimeBudget(_defaultTimeoutInSeconds);
            using (var operation = Log.OnEnterAndConfirmOnExit())
            {
                var processor = new BufferInliningTransformer();
                var processedRequest = await processor.TransformAsync(request, budget);
                Dictionary<string, (SourceFile Destination, TextSpan Region)> viewPorts = null;
                IEnumerable<(SerializableDiagnostic Diagnostic, string ErrorMessage)> processedDiagnostics;
                CommandLineResult commandLineResult = null;
                Exception exception = null;
                string exceptionMessage = null;
                OmnisharpEmitResponse emitResponse = null;
                emitResponse = await Emit(request, budget);

                if (emitResponse.Body.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
                {
                    emitResponse = await Emit(processedRequest, budget);

                    if (emitResponse.Body.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
                    {
                        viewPorts = processor.ExtractViewPorts(processedRequest);
                        processedDiagnostics = ReconstructDiagnosticLocations(emitResponse.Body.Diagnostics, viewPorts, BufferInliningTransformer.PaddingSize).ToArray();

                        return new RunResult(
                            false,
                            processedDiagnostics
                                .Where(d => d.Diagnostic.Severity == DiagnosticSeverity.Error)
                                .Select(d => d.ErrorMessage)
                                .ToArray(),
                            diagnostics: processedDiagnostics.Select(d => d.Diagnostic).ToArray());
                    }

                    var dotnet = new MLS.Agent.Tools.Dotnet(_workspace.Directory);

                    commandLineResult = await dotnet.Execute(emitResponse.Body.OutputAssemblyPath, budget);

                    if (commandLineResult.Exception != null)
                    {
                        exceptionMessage = commandLineResult.Exception.ToString();
                    }
                    else if (commandLineResult.Error.Count > 0)
                    {
                        exceptionMessage = string.Join(Environment.NewLine, commandLineResult.Error);
                    }
                }

                var dotnet = new MLS.Agent.Tools.Dotnet(_workspace.Directory);

                commandLineResult = await dotnet.Execute(emitResponse.Body.OutputAssemblyPath, budget);

                budget.RecordEntry(UserCodeCompletedBudgetEntryName);

                if (commandLineResult.ExitCode == 124)
                {
                    throw new BudgetExceededException(budget);
                }

                if (commandLineResult.Exception != null)
                {
                    exceptionMessage = commandLineResult.Exception.ToString();
                }
                else if (commandLineResult.Error.Count > 0)
                {
                    exceptionMessage = string.Join(Environment.NewLine, commandLineResult.Error);
                }

                if (emitResponse?.Body.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error) == true
                    && viewPorts == null)
                {
                    viewPorts = processor.ExtractViewPorts(processedRequest);
                }
                processedDiagnostics = ReconstructDiagnosticLocations(emitResponse?.Body.Diagnostics, viewPorts, BufferInliningTransformer.PaddingSize).ToArray();
                var runResult = new RunResult(
                    succeeded: !exception.IsConsideredRunFailure(),
                    output: commandLineResult?.Output,
                    exception: exceptionMessage ?? exception.ToDisplayString(),
                    diagnostics: processedDiagnostics.Select(d => d.Diagnostic).ToArray());

                operation.Complete(runResult, budget);
                return runResult;

            }
        }
        private static (SerializableDiagnostic, string) AlignDiagnosticLocation(KeyValuePair<string, (SourceFile Destination, TextSpan Region)> target, Diagnostic diagnostic, int paddingSize)
        {
            // offest of the buffer int othe original source file
            var offset = target.Value.Region.Start;
            // span of content injected in the buffer viewport
            var selectionSpan = new TextSpan(offset + paddingSize, target.Value.Region.Length - (2 * paddingSize));

            // aligned offset of the diagnostic entry
            var start = diagnostic.Location.SourceSpan.Start - selectionSpan.Start;
            var end = diagnostic.Location.SourceSpan.End - selectionSpan.Start;
            // line containing the diagnostic in the original source file
            var line = target.Value.Destination.Text.Lines[diagnostic.Location.MappedLineSpan.StartLinePosition.Line];

            // first line of the region from the soruce file
            var lineOffest = 0;

            foreach (var regionLine in target.Value.Destination.Text.GetSubText(selectionSpan).Lines)
            {
                if (regionLine.ToString() == line.ToString())
                {
                    break;
                }

                lineOffest++;
            }

            var bufferTextSource = SourceFile.Create(target.Value.Destination.Text.GetSubText(selectionSpan).ToString());
            var lineText = line.ToString();
            var partToFind = lineText.Substring(diagnostic.Location.MappedLineSpan.Span.Start.Character);
            var charOffset = bufferTextSource.Text.Lines[lineOffest].ToString().IndexOf(partToFind, StringComparison.Ordinal);
            var location = new { Line = lineOffest + 1, Char = charOffset + 1 };

            var errorMessage = $"({location.Line},{location.Char}): error {diagnostic.Id}: {diagnostic.Message}";

            var processedDiagnostic = (new SerializableDiagnostic(
                    start,
                    end,
                    diagnostic.Message,
                    diagnostic.Severity,
                    diagnostic.Id),
                errorMessage);
            return processedDiagnostic;
        }
        private static IEnumerable<(SerializableDiagnostic, string)> ReconstructDiagnosticLocations(IEnumerable<Diagnostic> bodyDiagnostics,
            Dictionary<string, (SourceFile Destination, TextSpan Region)> viewPortsByBufferId, int paddingSize)
        {
            var diagnostics = bodyDiagnostics ?? Enumerable.Empty<Diagnostic>();
            foreach (var diagnostic in diagnostics)
            {
                var diagnosticPath = diagnostic.Location.MappedLineSpan.Path;
                if (viewPortsByBufferId == null || viewPortsByBufferId.Count == 0)
                {
                    var errorMessage = diagnostic.ToString();
                    yield return (new SerializableDiagnostic(diagnostic), errorMessage);
                }
                else
                {
                    var target = viewPortsByBufferId
                        .Where(e => e.Key.Contains("@") && diagnosticPath.EndsWith(e.Value.Destination.Name))
                        .FirstOrDefault(e => e.Value.Region.Contains(diagnostic.Location.SourceSpan.Start));

                    if (!target.Value.Region.IsEmpty)
                    {
                        var processedDiagnostic = AlignDiagnosticLocation(target, diagnostic, paddingSize);
                        yield return processedDiagnostic;
                    }
                    else
                    {
                        var errorMessage = diagnostic.ToString();
                        yield return (new SerializableDiagnostic(diagnostic), errorMessage);
                    }
                }
            }
        }

        private async Task<OmnisharpEmitResponse> Emit(Models.Execution.Workspace request, Budget budget = null)
        {
            await EnsureInitializedAndNotDisposed(budget);

            foreach (var sourceFile in request.SourceFiles)
            {
                var file = new FileInfo(Path.Combine(_workspace.Directory.FullName, sourceFile.Name));

                var text = sourceFile.Text.ToString();

                if (!file.Exists)
                {
                    File.WriteAllText(file.FullName, text);
                }

                await _omniSharpServer.UpdateBuffer(file, text);
            }

            var emitResponse = await _omniSharpServer.Emit(budget);

            budget.RecordEntryAndThrowIfBudgetExceeded();

            return emitResponse;
        }

        public Task<CompletionResult> GetCompletionList(CompletionRequest request)
        {
            throw new NotImplementedException();
        }

        public async Task<DiagnosticResult> GetDiagnostics(Models.Execution.Workspace request)
        {
            var budget = new Budget();
            var processor = new BufferInliningTransformer();
            var processedRequest = await processor.TransformAsync(request, budget);
            var emitResult = await Emit(processedRequest, new Budget());
            SerializableDiagnostic[] diagnostics;
            if (emitResult.Body.Diagnostics.Any())
            {

                var viewPorts = processor.ExtractViewPorts(processedRequest);
                IEnumerable<(SerializableDiagnostic Diagnostic, string ErrorMessage)> processedDiagnostics = ReconstructDiagnosticLocations(emitResult.Body.Diagnostics, viewPorts, BufferInliningTransformer.PaddingSize).ToArray();
                diagnostics = processedDiagnostics?.Select(d => d.Diagnostic).ToArray();
            }

            else
            {
                diagnostics = emitResult.Body.Diagnostics.Select(d => new SerializableDiagnostic(d)).ToArray();
            }

            return new DiagnosticResult(diagnostics);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _initializationBudget.Cancel();
                _omniSharpServer?.Dispose();
            }
        }
    }
}
