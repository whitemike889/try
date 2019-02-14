using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Clockwise;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using MLS.Project.Execution;
using MLS.Project.Extensions;
using MLS.Project.Transformations;
using MLS.Protocol;
using MLS.Protocol.Diagnostics;
using MLS.Protocol.Execution;
using Workspace = MLS.Protocol.Execution.Workspace;

namespace WorkspaceServer.Transformations
{
    public static class DiagnosticTransformer
    {
        public static SerializableDiagnostic[] MapDiagnostics(
            this Workspace workspace,
            BufferId activeBufferId,
            Compilation compilation,
            Budget budget = null)
        {
            if (compilation == null)
            {
                throw new ArgumentNullException(nameof(compilation));
            }
            var diagnostics = compilation.GetDiagnostics()
                .Where(d => d.Id != "CS7022")
                .ToArray();

            return workspace.MapDiagnostics(activeBufferId, diagnostics, budget);
        }


        public static SerializableDiagnostic[] MapDiagnostics(
            this Workspace workspace,
            BufferId activeBufferId,
            Diagnostic[] diagnostics,
            Budget budget = null)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            if (activeBufferId == null)
            {
                return diagnostics.Select(d => d.ToSerializableDiagnostic()).ToArray();
            }

            if (diagnostics == null  || diagnostics.Length ==0)
            {
                return null;
            }

            budget = budget ?? new Budget();
            
            var viewPorts = workspace.ExtractViewPorts().ToList();

            budget.RecordEntry();

            var paddingSize = BufferInliningTransformer.PaddingSize;

            return ReconstructDiagnosticLocations().ToArray();

            IEnumerable<SerializableDiagnostic> ReconstructDiagnosticLocations()
            {
                foreach (var diagnostic in diagnostics.Where(d => d.Id != "CS7022"))
                {
                    var filePath = diagnostic.Location.SourceTree?.FilePath;

                    // hide warnings that are not within the visible code
                    if (!diagnostic.IsError() &&
                        !string.IsNullOrWhiteSpace(filePath))
                    {
                        if (Path.GetFileName(filePath) != Path.GetFileName(activeBufferId.FileName))
                        {
                            continue;
                        }
                    }

                    var lineSpan = diagnostic.Location.GetMappedLineSpan();
                    var lineSpanPath = lineSpan.Path;

                    if (viewPorts.Count == 0)
                    {
                        var errorMessage = RelativizeDiagnosticMessage();

                        yield return diagnostic.ToSerializableDiagnostic(errorMessage);
                    }
                    else
                    {
                        var target = viewPorts
                                     .Where(e => e.BufferId.RegionName != null &&
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
                    }

                    string RelativizeDiagnosticMessage()
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
                        }

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

            foreach (var regionLine in sourceText.Lines)
            {
                if (regionLine.ToString() == line.ToString())
                {
                    var lineText = line.ToString();
                    var partToFind = lineText.Substring(diagnostic.Location.GetMappedLineSpan().Span.Start.Character);
                    var charOffset = sourceText.Lines[lineOffset].ToString().IndexOf(partToFind, StringComparison.Ordinal);

                    var errorMessage = $"({lineOffset + 1},{charOffset + 1}): error {diagnostic.Id}: {diagnostic.GetMessage()}";

                    return new SerializableDiagnostic(
                        start,
                        end,
                        errorMessage,
                        diagnostic.ConvertSeverity(),
                        diagnostic.Id);
                }

                lineOffset++;
            }

            return null;
        }
    }
}
