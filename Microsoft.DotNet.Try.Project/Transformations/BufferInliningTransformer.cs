using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Clockwise;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.DotNet.Try.Project.Execution;
using Microsoft.DotNet.Try.Project.Extensions;
using Microsoft.DotNet.Try.Protocol.Execution;
using Workspace = Microsoft.DotNet.Try.Protocol.Execution.Workspace;

namespace Microsoft.DotNet.Try.Project.Transformations
{
    public class BufferInliningTransformer : IWorkspaceTransformer
    {
        private static readonly string ProcessorName = typeof(BufferInliningTransformer).Name;
        private static readonly string Padding = "\n";

        public static int PaddingSize => Padding.Length;

        public async Task<Workspace> TransformAsync(Workspace source, Budget timeBudget = null)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var (files, buffers) = await InlineBuffersAsync(source, timeBudget);

            return new Workspace(
                workspaceType: source.WorkspaceType,
                files: files,
                buffers: buffers,
                usings: source.Usings,
                includeInstrumentation: source.IncludeInstrumentation);
        }

        private static async Task<(Workspace.File[] files, Workspace.Buffer[] buffers)> InlineBuffersAsync(Workspace source, Budget timeBudget)
        {
            var files = (source.Files ?? Array.Empty<Workspace.File>()).ToDictionary(f => f.Name, f =>
             {
                 if (string.IsNullOrEmpty(f.Text) && File.Exists(f.Name))
                 {
                     return SourceFile.Create(File.ReadAllText(f.Name), f.Name);
                 }

                 return f.ToSourceFile();
             });

            var buffers = new List<Workspace.Buffer>();
            foreach (var sourceBuffer in source.Buffers)
            {
                var bufferFileName = sourceBuffer.Id.FileName;
                if (!files.ContainsKey(bufferFileName) && File.Exists(bufferFileName))
                {
                    var sourceFile = SourceFile.Create(File.ReadAllText(bufferFileName), bufferFileName);
                    files[bufferFileName] = sourceFile;
                }

                if (!string.IsNullOrWhiteSpace(sourceBuffer.Id.RegionName))
                {
                    var normalizedBufferId = sourceBuffer.Id.GetNormalized();
                    var injectionPoint = sourceBuffer.Id.GetInjectionPoint();
                    var viewPorts = files.Select(f => f.Value).ExtractViewports();
                    if (viewPorts.SingleOrDefault(viewport => viewport.BufferId == normalizedBufferId) is Viewport viewPort)
                    {
                        await InjectBuffer(viewPort, sourceBuffer, buffers, files, injectionPoint);
                    }
                    else
                    {
                        throw new ArgumentException($"Could not find specified buffer: {sourceBuffer.Id}");
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
        private static Task InjectBuffer(Viewport viewPort, Workspace.Buffer sourceBuffer, ICollection<Workspace.Buffer> buffers, IDictionary<string, SourceFile> files,
            BufferInjectionPoints bufferIdInjectionPoints)
        {
            TextSpan targetSpan;
            switch (bufferIdInjectionPoints)
            {
                case BufferInjectionPoints.Before:
                    targetSpan = CreateTextSpanBefore(viewPort.OuterRegion);
                    break;
                case BufferInjectionPoints.After:
                    targetSpan = CreateTextSpanAfter(viewPort.OuterRegion);
                    break;
                default:
                    targetSpan = viewPort.Region;
                    break;
            }
            return InjectBufferAtSpan(viewPort, sourceBuffer, buffers, files, targetSpan);
        }

        private static TextSpan CreateTextSpanAfter(TextSpan viewPortRegion)
        {
            return new TextSpan(viewPortRegion.End, 0);
        }

        private static TextSpan CreateTextSpanBefore(TextSpan viewPortRegion)
        {
            return new TextSpan(viewPortRegion.Start, 0);
        }

        private static async Task InjectBufferAtSpan(Viewport viewPort, Workspace.Buffer sourceBuffer, ICollection<Workspace.Buffer> buffers, IDictionary<string, SourceFile> files, TextSpan span)
        {
            var tree = CSharpSyntaxTree.ParseText(viewPort.Destination.Text.ToString());
            var textChange = new TextChange(
                span,
                $"{Padding}{sourceBuffer.Content}{Padding}");

            var txt = tree.WithChangedText(tree.GetText().WithChanges(textChange));

            var offset = span.Start + PaddingSize;

            var newCode = (await txt.GetTextAsync()).ToString();

            buffers.Add(new Workspace.Buffer(
                sourceBuffer.Id,
                sourceBuffer.Content,
                sourceBuffer.Position,
                offset));
            files[viewPort.Destination.Name] = SourceFile.Create(newCode, viewPort.Destination.Name);
        }

        private static Task InjectBufferBeforeViewport(Viewport viewPort, Workspace.Buffer sourceBuffer, ICollection<Workspace.Buffer> buffers, IDictionary<string, SourceFile> files)
        {
            var span = viewPort.Region;
            return InjectBufferAtSpan(viewPort, sourceBuffer, buffers, files, span);
        }

        private static Task InjectBufferAfterViewport(Viewport viewPort, Workspace.Buffer sourceBuffer, ICollection<Workspace.Buffer> buffers, IDictionary<string, SourceFile> files)
        {
            var span = viewPort.Region;
            return InjectBufferAtSpan(viewPort, sourceBuffer, buffers, files, span);
        }
    }
}
