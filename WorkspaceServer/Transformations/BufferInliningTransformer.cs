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
    public class BufferInliningTransformer : IWorkspaceTransformer
    {
        private static readonly string ProcessorName = typeof(BufferInliningTransformer).Name;
        private static readonly string Padding = "\n";

        public static int PaddingSize => Padding.Length;

        public async Task<Workspace> TransformAsync(Workspace source, Budget timeBudget = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var results = await InlineBuffersAsync(source, timeBudget);

            return new Workspace(
                workspaceType: source.WorkspaceType, 
                files: results.files,
                buffers: results.buffers,
                usings: source.Usings,
                includeInstrumentation: source.IncludeInstrumentation);
        }

        public IReadOnlyCollection<Viewport> ExtractViewPorts(Workspace ws)
        {
            if (ws == null) throw new ArgumentNullException(nameof(ws));

            var files = ws.GetSourceFiles();

            return ExtractViewPorts(files);
        }

        private static async Task<(Workspace.File[] files, Workspace.Buffer[] buffers)> InlineBuffersAsync(Workspace source, Budget timeBudget)
        {
            var files = source.GetSourceFiles().ToDictionary(f => f.Name);
            var buffers = new List<Workspace.Buffer>();
            foreach (var sourceBuffer in source.Buffers)
            {
                if (!string.IsNullOrWhiteSpace(sourceBuffer.Id.RegionName))
                {
                    var viewPorts = ExtractViewPorts(files.Values);
                    if (viewPorts.SingleOrDefault(p => p.BufferId == sourceBuffer.Id.ToString()) is Viewport viewPort)
                    {
                        var tree = CSharpSyntaxTree.ParseText(viewPort.Destination.Text.ToString());
                        var textChange = new TextChange(
                            viewPort.Region,
                            $"{Padding}{sourceBuffer.Content}{Padding}");

                        var txt = tree.WithChangedText(tree.GetText().WithChanges(textChange));

                        var offset = viewPort.Region.Start + PaddingSize;

                        var newCode = (await txt.GetTextAsync()).ToString();

                        buffers.Add(new Workspace.Buffer(
                                        sourceBuffer.Id,
                                        sourceBuffer.Content,
                                        sourceBuffer.Position,
                                        offset));
                        files[viewPort.Destination.Name] = SourceFile.Create(newCode, viewPort.Destination.Name);
                    }
                    else
                    {
                        throw new ArgumentException($"Could not find specified viewport {sourceBuffer.Id}");
                    }
                }
                else
                {
                    files[sourceBuffer.Id.FileName] = SourceFile.Create(sourceBuffer.Content, sourceBuffer.Id.FileName);
                    buffers.Add(sourceBuffer);
                }
            }

            var processedFiles = files.Values.Select(sf => new Workspace.File(sf.Name, sf.Text.ToString())).ToArray();
            var processedBuffers = buffers.ToArray();
            timeBudget?.RecordEntry(ProcessorName);
            return (processedFiles, processedBuffers);
        }

        private static IReadOnlyCollection<Viewport> ExtractViewPorts(
            IReadOnlyCollection<SourceFile> files)
        {
            var viewPorts = new Dictionary<BufferId, Viewport>();

            foreach (var sourceFile in files)
            {
                var code = sourceFile.Text;
                var fileName = sourceFile.Name;
                var regions = ExtractRegions(code, fileName);

                foreach (var region in regions)
                {
                    viewPorts.Add(
                        region.bufferId, 
                        new Viewport(sourceFile, region.span, region.bufferId));
                }
            }

            return viewPorts.Values;
        }

        private static IEnumerable<(BufferId bufferId, TextSpan span)> ExtractRegions(SourceText code, string fileName)
        {
            IEnumerable<(SyntaxTrivia startRegion, SyntaxTrivia endRegion, BufferId bufferId)> FindRegions(SyntaxNode syntaxNode)
            {
                var nodesWithRegionDirectives =
                    from node in syntaxNode.DescendantNodesAndTokens()
                    where node.HasLeadingTrivia
                    from leadingTrivia in node.GetLeadingTrivia()
                    where leadingTrivia.Kind() == SyntaxKind.RegionDirectiveTrivia ||
                          leadingTrivia.Kind() == SyntaxKind.EndRegionDirectiveTrivia
                    select node;

                var stack = new Stack<SyntaxTrivia>();
                var processedSpans = new HashSet<TextSpan>();

                foreach (var nodeWithRegionDirective in nodesWithRegionDirectives)
                {
                    var triviaList = nodeWithRegionDirective.GetLeadingTrivia();

                    foreach (var currentTrivia in triviaList)
                    {
                        if (processedSpans.Add(currentTrivia.FullSpan))
                        {
                            if (currentTrivia.Kind() == SyntaxKind.RegionDirectiveTrivia)
                            {
                                stack.Push(currentTrivia);
                            }
                            else if (currentTrivia.Kind() == SyntaxKind.EndRegionDirectiveTrivia)
                            {
                                var start = stack.Pop();
                                var regionName = start.ToFullString().Replace("#region", string.Empty).Trim();
                                yield return (start, currentTrivia, new BufferId(fileName, regionName));
                            }
                        }
                    }
                }
            }

            var sourceCodeText = code.ToString();
            var root = CSharpSyntaxTree.ParseText(sourceCodeText).GetRoot();

            foreach (var (startRegion, endRegion, label) in FindRegions(root))
            {
                var start = startRegion.GetLocation().SourceSpan.End;
                var length = endRegion.GetLocation().SourceSpan.Start -
                             startRegion.GetLocation().SourceSpan.End;
                var loc = new TextSpan(start, length);
                yield return (label, loc);
            }
        }
    }
}
