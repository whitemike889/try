using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using WorkspaceServer.Models.Execution;

namespace WorkspaceServer.Processors
{
    public class BufferInliningProcessor : IWorksapceProcessor
    {
        public async Task<WorkspaceRunRequest> ProcessAsync(WorkspaceRunRequest source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var results = await InlineBuffersAsync(source);

            return new WorkspaceRunRequest(workspaceType: source.WorkspaceType, files: results);
        }

        public Dictionary<string, (SourceFile Destination, TextSpan Region)> ExtractViewPorts(WorkspaceRunRequest ws)
        {
            if (ws == null) throw new ArgumentNullException(nameof(ws));

            var files = ws.SourceFiles;

            return ExtractViewPorts(files);
        }
        private static async Task<WorkspaceRunRequest.File[]> InlineBuffersAsync(WorkspaceRunRequest source)
        {
            var files = source.SourceFiles.ToDictionary(f => f.Name);
            foreach (var sourceBuffer in source.Buffers)
            {
                var viewPorts = ExtractViewPorts(files.Values);
                if (viewPorts.TryGetValue(sourceBuffer.Id, out var viewPort))
                {
                    var tree = CSharpSyntaxTree.ParseText(viewPort.Destination.Text.ToString());
                    var textChange = new TextChange(
                        viewPort.Region,
                        $"{Environment.NewLine}{sourceBuffer.Content}{Environment.NewLine}");

                    var txt = tree.GetText()
                        .WithChanges(
                            textChange);

                    var newCode = txt.ToString();

                    files[viewPort.Destination.Name] = SourceFile.Create(newCode, viewPort.Destination.Name);
                }
                else if (sourceBuffer.Id == string.Empty)
                {
                    files["Program.cs"] = SourceFile.Create(sourceBuffer.Content, "Program.cs");
                }
            }

            return files.Values.Select(sf => new WorkspaceRunRequest.File(sf.Name, sf.Text.ToString())).ToArray();
        }


        private static Dictionary<string, (SourceFile Destination, TextSpan Region)> ExtractViewPorts(
            IReadOnlyCollection<SourceFile> files)
        {
            var viewPorts = new Dictionary<string, (SourceFile Destination, TextSpan Region)>();


            if (files.Count == 0) return viewPorts;

            foreach (var sourceFile in files)
            {
                var code = sourceFile.Text;

                var regions = ExtractRegions(code);

                foreach (var region in regions)
                {
                    viewPorts.Add(region.regionName, (sourceFile, region.span));
                }
            }

            return viewPorts;
        }

        private static IEnumerable<(string regionName, TextSpan span)> ExtractRegions(SourceText code)
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

                var triviaToRemove = new List<(SyntaxTrivia startRegion, SyntaxTrivia endRegion, string label)>();
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
                            triviaToRemove.Add(
                                (start, currentTrivia, start.ToFullString().Replace("#region", string.Empty).Trim()));
                        }
                    }
                }

                return triviaToRemove;
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
