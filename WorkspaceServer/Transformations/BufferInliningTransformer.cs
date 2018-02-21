using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Clockwise;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using WorkspaceServer.Models.Execution;
using Workspace = WorkspaceServer.Models.Execution.Workspace;

namespace WorkspaceServer.Transformations
{
    public class BufferInliningTransformer : IWorksapceTransformer
    {
        private static readonly string ProcessorName = typeof(BufferInliningTransformer).Name;
        private static readonly string Padding = Environment.NewLine;
        public static int PaddingSize => Padding.Length;
        public async Task<Workspace> TransformAsync(Workspace source, Budget timeBudget = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var results = await InlineBuffersAsync(source, timeBudget);

            return new Workspace(workspaceType: source.WorkspaceType, files: results.files, buffers: results.buffers);
        }

        public Dictionary<string, (SourceFile Destination, TextSpan Region)> ExtractViewPorts(Workspace ws)
        {
            if (ws == null) throw new ArgumentNullException(nameof(ws));

            var files = ws.SourceFiles;

            return ExtractViewPorts(files);
        }

        private static async Task<(Workspace.File[] files, Workspace.Buffer[] buffers)> InlineBuffersAsync(Workspace source, Budget timeBudget)
        {
            var files = source.SourceFiles.ToDictionary(f => f.Name);
            var buffers = new List<Workspace.Buffer>();
            foreach (var sourceBuffer in source.Buffers)
            {
                if (sourceBuffer.Id.Contains("@"))
                {
                    var viewPorts = ExtractViewPorts(files.Values);
                    if (viewPorts.TryGetValue(sourceBuffer.Id, out var viewPort))
                    {
                        var tree = CSharpSyntaxTree.ParseText(viewPort.Destination.Text.ToString());
                        var textChange = new TextChange(
                            viewPort.Region,
                            $"{Padding}{sourceBuffer.Content}{Padding}");


                        var txt = tree.WithChangedText(tree.GetText().WithChanges(textChange));

                        var offset = tree.GetChangedSpans(txt).FirstOrDefault().Start;

                        var newCode = (await txt.GetTextAsync()).ToString();

                        buffers.Add(new Workspace.Buffer(sourceBuffer.Id, sourceBuffer.Content, offset));
                        files[viewPort.Destination.Name] = SourceFile.Create(newCode, viewPort.Destination.Name);
                    }

                }
                else if (sourceBuffer.Id == string.Empty)
                {
                    files["Program.cs"] = SourceFile.Create(sourceBuffer.Content, "Program.cs");
                    buffers.Add(new Workspace.Buffer(sourceBuffer.Id, sourceBuffer.Content, 0));
                }
                else
                {
                    files[sourceBuffer.Id] = SourceFile.Create(sourceBuffer.Content, sourceBuffer.Id);
                    buffers.Add(new Workspace.Buffer(sourceBuffer.Id, sourceBuffer.Content, 0));
                }
            }

            var processedFiles = files.Values.Select(sf => new Workspace.File(sf.Name, sf.Text.ToString())).ToArray();
            var processedBuffers = buffers.ToArray();
            timeBudget?.RecordEntry(ProcessorName);
            return (processedFiles, processedBuffers);
        }


        private static Dictionary<string, (SourceFile Destination, TextSpan Region)> ExtractViewPorts(
            IReadOnlyCollection<SourceFile> files)
        {
            var viewPorts = new Dictionary<string, (SourceFile Destination, TextSpan Region)>();

            if (files.Count == 0)
            {
                return viewPorts;
            }

            foreach (var sourceFile in files)
            {
                var code = sourceFile.Text;
                var fileName = sourceFile.Name;
                var regions = ExtractRegions(code, fileName);

                foreach (var region in regions)
                {
                    viewPorts.Add(region.regionName, (sourceFile, region.span));
                }
            }

            return viewPorts;
        }

        private static IEnumerable<(string regionName, TextSpan span)> ExtractRegions(SourceText code, string fileName)
        {
            List<(SyntaxTrivia startRegion, SyntaxTrivia endRegion, string label)> FindRegions(SyntaxNode syntaxNode)
            {
                var nodesWithRegionDirectives =
                    from node in syntaxNode.DescendantNodesAndTokens()
                    where node.HasLeadingTrivia
                    from leadingTrivia in node.GetLeadingTrivia()
                    where leadingTrivia.Kind() == SyntaxKind.RegionDirectiveTrivia ||
                          leadingTrivia.Kind() == SyntaxKind.EndRegionDirectiveTrivia
                    select node;

                var viewPorts = new List<(SyntaxTrivia startRegion, SyntaxTrivia endRegion, string label)>();
                var stack = new Stack<SyntaxTrivia>();
                var processedSpans = new HashSet<TextSpan>();
                foreach (var nodeWithRegionDirective in nodesWithRegionDirectives)
                {
                    var triviaList = nodeWithRegionDirective.GetLeadingTrivia();

                    foreach (var currentTrivia in triviaList)
                    {
                        if (!processedSpans.Add(currentTrivia.FullSpan)) continue;

                        if (currentTrivia.Kind() == SyntaxKind.RegionDirectiveTrivia)
                        {
                            stack.Push(currentTrivia);
                        }
                        else if (currentTrivia.Kind() == SyntaxKind.EndRegionDirectiveTrivia)
                        {
                            var start = stack.Pop();
                            var regionName = start.ToFullString().Replace("#region", string.Empty).Trim();
                            var viewPortId = $"{fileName}@{regionName}";
                            viewPorts.Add(
                                (start, currentTrivia, viewPortId));
                        }
                    }
                }

                return viewPorts;
            }

            var sourceCodeText = code.ToString();
            var root = CSharpSyntaxTree.ParseText(sourceCodeText).GetRoot();
            var regions = new List<(string regionName, TextSpan span)>();

            foreach (var (startRegion, endRegion, label) in FindRegions(root))
            {
                var start = startRegion.GetLocation().SourceSpan.End;
                var length = endRegion.GetLocation().SourceSpan.Start -
                             startRegion.GetLocation().SourceSpan.End;
                var loc = new TextSpan(start, length);
                regions.Add((label, loc));
            }

            return regions;
        }
    }
}
