using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Clockwise;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;
using MLS.Agent.Tools;
using WorkspaceServer.Models.Completion;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Transformations;
using Diagnostic = OmniSharp.Client.Diagnostic;
using Workspace = MLS.Agent.Tools.Workspace;
using OmnisharpEmitResponse = OmniSharp.Client.Commands.OmniSharpResponseMessage<OmniSharp.Client.Commands.EmitResponse>;
using TextSpan = Microsoft.CodeAnalysis.Text.TextSpan;

namespace WorkspaceServer.Servers.Dotnet
{
    public class DotnetWorkspaceServer : IWorkspaceServer, IDisposable
    {
        private readonly Workspace _workspace;
        private readonly OmniSharpServer _omniSharpServer;

        private bool _disposed;

        public DotnetWorkspaceServer(Workspace workspace)
        {
            _workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));

            _omniSharpServer = new OmniSharpServer(
                _workspace.Directory,
                Paths.EmitPlugin,
                logToPocketLogger: false);
        }

        public async Task EnsureInitializedAndNotDisposed(TimeBudget budget = null)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(DotnetWorkspaceServer));
            }

            budget?.RecordEntryAndThrowIfBudgetExceeded();

            await _workspace.EnsureCreated(budget);

            await _workspace.EnsureBuilt(budget);

            await _omniSharpServer.WorkspaceReady(budget);
        }

        public async Task<RunResult> Run(Models.Execution.Workspace request, TimeBudget budget = null)
        {
            budget = budget ?? TimeBudget.Unlimited();
            var processor = new BufferInliningTransformer();
            var processedRequest = await processor.ProcessAsync(request);
            var viewPorts = processor.ExtractViewPorts(processedRequest);
            CommandLineResult result = null;
            Exception exception = null;
            string exceptionMessage = null;
            OmnisharpEmitResponse emitResponse = null;
            IEnumerable<(SerializableDiagnostic Diagnostic,string ErrorMessage)> processedDiagnostics;

            try
            {
                await EnsureInitializedAndNotDisposed(budget);

                emitResponse = await Emit(processedRequest, budget);

                if (emitResponse.Body.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
                {
                    
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

                result = dotnet.Execute(emitResponse.Body.OutputAssemblyPath, budget);

                if (result.Exception != null)
                {
                    exceptionMessage = result.Exception.ToString();
                }
                else if (result.Error.Count > 0)
                {
                    exceptionMessage = string.Join(Environment.NewLine, result.Error);
                }
            }
            catch (TimeoutException timeoutException)
            {
                exception = timeoutException;
            }
            catch (TimeBudgetExceededException)
            {
                exception = new TimeoutException(); 
            }
            catch (TaskCanceledException taskCanceledException)
            {
                exception = taskCanceledException;
            }

            processedDiagnostics = ReconstructDiagnosticLocations(emitResponse?.Body.Diagnostics, viewPorts, BufferInliningTransformer.PaddingSize).ToArray();
            return new RunResult(
                succeeded:  !(exception is TimeoutException) &&
                            !(exception is CompilationErrorException),
                output: result?.Output,
                exception: exceptionMessage ?? exception.ToDisplayString(),
                diagnostics: processedDiagnostics.Select(d => d.Diagnostic).ToArray());
        }

        private static IEnumerable<(SerializableDiagnostic ,string)> ReconstructDiagnosticLocations(IEnumerable<Diagnostic> bodyDiagnostics,
            Dictionary<string, (SourceFile Destination, TextSpan Region)> viewPorts, int paddingSize)
        {
            var diagnostics = bodyDiagnostics ?? Enumerable.Empty<Diagnostic>();
            foreach (var diagnostic in diagnostics)
            {
                var diagnosticPath = diagnostic.Location.MappedLineSpan.Path;
                var target = viewPorts
                    .Where(e => diagnosticPath.EndsWith(e.Value.Destination.Name))
                    .FirstOrDefault(e=>e.Value.Region.Contains(diagnostic.Location.SourceSpan.Start));

                if (!target.Value.Region.IsEmpty)
                {
                    // offest of the buffer int othe original source file
                    var offset = target.Value.Region.Start;
                    // span of content injected in the buffer viewport
                    var selectionSpan = new TextSpan(offset + paddingSize, target.Value.Region.Length - (2*paddingSize));
                  
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
                    var charOffset = bufferTextSource.Text.Lines[lineOffest].ToString().IndexOf(line.ToString().Substring(diagnostic.Location.MappedLineSpan.Span.Start.Character), StringComparison.Ordinal);
                    var location = new { Line = lineOffest + 1, Char = charOffset + 1 };

                    var errorMessage = $"({location.Line},{location.Char}): error {diagnostic.Id}: {diagnostic.Message}";

                    yield return (new SerializableDiagnostic(
                            start,
                            end,
                            diagnostic.Message,
                            diagnostic.Severity,
                            diagnostic.Id),
                        errorMessage);
                }
                else
                {
                    var errorMessage = diagnostic.ToString();
                    yield return (new SerializableDiagnostic(diagnostic), errorMessage);
                }
            }
        }

        private async Task<OmnisharpEmitResponse> Emit(Models.Execution.Workspace request, TimeBudget budget = null)
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

            return await _omniSharpServer.Emit(budget);
        }

        public Task<CompletionResult> GetCompletionList(CompletionRequest request)
        {
            throw new NotImplementedException();
        }

        public async Task<DiagnosticResult> GetDiagnostics(Models.Execution.Workspace request)
        {
            var emitResult = await Emit(request);
            var diagnostics = emitResult.Body.Diagnostics.Select(d => new SerializableDiagnostic(d)).ToArray();
            return new DiagnosticResult(diagnostics);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _omniSharpServer?.Dispose();
            }
        }
    }
}
