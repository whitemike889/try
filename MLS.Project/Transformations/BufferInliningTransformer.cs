using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Clockwise;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using MLS.Project.Execution;
using MLS.Project.Extensions;
using Workspace = MLS.Protocol.Execution.Workspace;

namespace MLS.Project.Transformations
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

        private static async Task<(Workspace.File[] files, Workspace.Buffer[] buffers)> InlineBuffersAsync(Workspace source, Budget timeBudget)
        {
            var files = source.GetSourceFiles().ToDictionary(f => f.Name);
            var buffers = new List<Workspace.Buffer>();
            foreach (var sourceBuffer in source.Buffers)
            {
                if (!string.IsNullOrWhiteSpace(sourceBuffer.Id.RegionName))
                {
                    var viewPorts = files.Select(f => f.Value).ExtractViewports();
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
    }
}
