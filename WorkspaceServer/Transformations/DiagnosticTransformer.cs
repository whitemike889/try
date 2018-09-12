using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using WorkspaceServer.Models;
using WorkspaceServer.Models.Execution;

namespace WorkspaceServer.Transformations
{
    public class DiagnosticTransformer
    {
        public static IEnumerable<SerializableDiagnostic> ReconstructDiagnosticLocations(
            IReadOnlyCollection<Diagnostic> diagnostics,
            IReadOnlyCollection<Viewport> viewPorts,
            int paddingSize)
        {
            if (diagnostics == null)
            {
                throw new ArgumentNullException(nameof(diagnostics));
            }

            if (viewPorts == null)
            {
                throw new ArgumentNullException(nameof(viewPorts));
            }

            foreach (var diagnostic in diagnostics)
            {
                var lineSpan = diagnostic.Location.GetMappedLineSpan();
                var lineSpanPath = lineSpan.Path;

                if (viewPorts.Count == 0)
                {
                    var errorMessage = RelativizeFilePath();

                    yield return new SerializableDiagnostic(diagnostic, errorMessage);
                }
                else
                {
                    var target = viewPorts
                                 .Where(e => e.Name.Contains("@") &&
                                             (!lineSpan.HasMappedPath || lineSpanPath.EndsWith(e.Destination.Name)))
                                 .FirstOrDefault(e => e.Region.Contains(diagnostic.Location.SourceSpan.Start));

                    if (target != null && !target.Region.IsEmpty)
                    {
                        var processedDiagnostic = AlignDiagnosticLocation(target, diagnostic, paddingSize);
                        if (processedDiagnostic != null)
                        {
                            yield return processedDiagnostic;
                        }
                    }
                    else
                    {
                        var errorMessage = RelativizeFilePath();

                        yield return new SerializableDiagnostic(diagnostic, errorMessage);
                    }
                }

                string RelativizeFilePath()
                {
                    var message = diagnostic.ToString();

                    if (!string.IsNullOrWhiteSpace(lineSpanPath))
                    {
                        var directoryPath = new FileInfo(lineSpanPath).Directory?.FullName ?? "";

                        if (!directoryPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                        {
                            directoryPath += Path.DirectorySeparatorChar;
                        }

                        if (message.StartsWith(directoryPath))
                        {
                            return message.Substring(directoryPath.Length);
                        }
                        else
                        {
                            return message;
                        }
                    }
                    else
                    {
                        return message;
                    }
                }
            }
        }

        private static SerializableDiagnostic AlignDiagnosticLocation(
            Viewport viewport, 
            Diagnostic diagnostic, 
            int paddingSize)
        {
            // offset of the buffer into the original source file
            var offset = viewport.Region.Start;
            // span of content injected in the buffer viewport
            var selectionSpan = new TextSpan(offset + paddingSize, viewport.Region.Length - (2 * paddingSize));

            // aligned offset of the diagnostic entry
            var start = diagnostic.Location.SourceSpan.Start - selectionSpan.Start;
            var end = diagnostic.Location.SourceSpan.End - selectionSpan.Start;
            // line containing the diagnostic in the original source file
            var line = viewport.Destination.Text.Lines[diagnostic.Location.GetMappedLineSpan().StartLinePosition.Line];

            // first line of the region from the source file
            var lineOffset = 0;
            var sourceText = viewport.Destination.Text.GetSubText(selectionSpan);

            var lines = sourceText.Lines;
            foreach (var regionLine in lines)
            {
                if (regionLine.ToString() == line.ToString())
                {
                    break;
                }

                lineOffset++;
            }
          
            var lineText = line.ToString();
            var partToFind = lineText.Substring(diagnostic.Location.GetMappedLineSpan().Span.Start.Character);
            var charOffset = sourceText.Lines[lineOffset].ToString().IndexOf(partToFind, StringComparison.Ordinal);

            var errorMessage = $"({lineOffset + 1},{charOffset + 1}): error {diagnostic.Id}: {diagnostic.GetMessage()}";

            return new SerializableDiagnostic(
                start,
                end,
                errorMessage,
                diagnostic.Severity,
                diagnostic.Id);
        }
    }
}