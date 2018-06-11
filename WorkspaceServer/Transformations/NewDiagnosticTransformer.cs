using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using WorkspaceServer.Models.Execution;

namespace WorkspaceServer.Transformations
{
    public class NewDiagnosticTransformer
    {
        public static IEnumerable<SerializableDiagnostic> ReconstructDiagnosticLocations(IEnumerable<Diagnostic> bodyDiagnostics,
            Dictionary<string, Viewport> viewPortsByBufferId, int paddingSize)
        {
            var diagnostics = bodyDiagnostics ?? Enumerable.Empty<Diagnostic>();
            foreach (var diagnostic in diagnostics)
            {
                if (diagnostic.Location == Location.None)
                {
                    yield return new SerializableDiagnostic(
                        1, 1, diagnostic.GetMessage(),
                        diagnostic.Severity, diagnostic.Id);
                    continue;
                }

                var diagnosticPath = diagnostic.Location.SourceTree.FilePath;
                if (viewPortsByBufferId == null || viewPortsByBufferId.Count == 0)
                {
                    var errorMessage = diagnostic.ToString();
                    yield return new SerializableDiagnostic(diagnostic);
                }
                else
                {
                    var target = viewPortsByBufferId
                        .Where(e => e.Key.Contains("@") && diagnosticPath.EndsWith(e.Value.Destination.Name))
                        .FirstOrDefault(e => e.Value.Region.Contains(diagnostic.Location.SourceSpan.Start));

                    if (target.Value != null && !target.Value.Region.IsEmpty)
                    {
                        var processedDiagnostic = AlignDiagnosticLocation(target, diagnostic, paddingSize);
                        yield return processedDiagnostic;
                    }
                    else
                    {
                        var errorMessage = diagnostic.ToString();
                        yield return new SerializableDiagnostic(diagnostic);
                    }
                }
            }
        }
        private static SerializableDiagnostic AlignDiagnosticLocation(KeyValuePair<string, Viewport> target, Diagnostic diagnostic, int paddingSize)
        {
            // offest of the buffer int othe original source file
            var offset = target.Value.Region.Start;
            // span of content injected in the buffer viewport
            var selectionSpan = new TextSpan(offset + paddingSize, target.Value.Region.Length - (2 * paddingSize));

            // aligned offset of the diagnostic entry
            var start = diagnostic.Location.SourceSpan.Start - selectionSpan.Start;
            var end = diagnostic.Location.SourceSpan.End - selectionSpan.Start;
            // line containing the diagnostic in the original source file
            var line = target.Value.Destination.Text.Lines[diagnostic.Location.GetMappedLineSpan().StartLinePosition.Line];

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
            var partToFind = lineText.Substring(diagnostic.Location.GetMappedLineSpan().Span.Start.Character);
            var charOffset = bufferTextSource.Text.Lines[lineOffest].ToString().IndexOf(partToFind, StringComparison.Ordinal);
            var location = new { Line = lineOffest + 1, Char = charOffset + 1 };

            var errorMessage = $"({location.Line},{location.Char}): error {diagnostic.Id}: {diagnostic.GetMessage()}";

            var processedDiagnostic = new SerializableDiagnostic(
                    start,
                    end,
                    diagnostic.GetMessage(),
                    diagnostic.Severity,
                    diagnostic.Id);
            return processedDiagnostic;
        }
    }
}