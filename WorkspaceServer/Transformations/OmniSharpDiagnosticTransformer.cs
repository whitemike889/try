using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Text;
using WorkspaceServer.Models.Execution;

namespace WorkspaceServer.Transformations
{
    public class OmniSharpDiagnosticTransformer
    {
        public static IEnumerable<(SerializableDiagnostic, string)> ReconstructDiagnosticLocations(IEnumerable<OmniSharp.Client.Diagnostic> bodyDiagnostics,
            Dictionary<string, Viewport> viewPortsByBufferId, int paddingSize)
        {
            var diagnostics = bodyDiagnostics ?? Enumerable.Empty<OmniSharp.Client.Diagnostic>();
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
        private static (SerializableDiagnostic, string) AlignDiagnosticLocation(KeyValuePair<string, Viewport> target, OmniSharp.Client.Diagnostic diagnostic, int paddingSize)
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
    }
}