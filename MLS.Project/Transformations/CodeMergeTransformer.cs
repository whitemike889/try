using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Clockwise;
using MLS.Protocol.Execution;

namespace MLS.Project.Transformations
{
    public class CodeMergeTransformer : IWorkspaceTransformer
    {
        private static readonly string ProcessorName = typeof(CodeMergeTransformer).Name;
        private static readonly string Padding = "\n";

        public Task<Workspace> TransformAsync(Workspace source, Budget timeBudget = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var files = (source.Files?? Array.Empty<Workspace.File>())
                .GroupBy(file => file.Name)
                .Select(fileGroup => { return new Workspace.File(fileGroup.Key, string.Join(Padding, fileGroup.OrderBy(f => f.Order).Select(f => f.Text))); });

            var buffers = (source.Buffers ?? Array.Empty<Workspace.Buffer>())
                .GroupBy(buffer => buffer.Id)
                .Select(bufferGroup => MergeBuffers(bufferGroup.Key, bufferGroup));


            var workspace = new Workspace(
                workspaceType: source.WorkspaceType,
                usings: source.Usings, 
                files: files.ToArray(),
                buffers: buffers.ToArray());

            timeBudget?.RecordEntry(ProcessorName);

            return Task.FromResult(workspace);
        }

        private Workspace.Buffer MergeBuffers(BufferId id, IEnumerable<Workspace.Buffer> buffers)
        {
            var position = 0;
            var content = string.Empty;
            var sortId = 0;
            foreach (var buffer in buffers.OrderBy(buffer => buffer.Order))
            {
                sortId = buffer.Order;
                if (buffer.Position != 0)
                {
                    position = content.Length + buffer.Position;
                    content = $"{content}{buffer.Content}{Padding}";
                }

                content = content.Substring(0, content.Length - Padding.Length);
            }
            return new Workspace.Buffer(id,content,position:position, order:sortId);
        }
    }
}