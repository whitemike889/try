using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Clockwise;
using MLS.Protocol.Execution;

namespace Microsoft.DotNet.Try.Project.Transformations
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

            var files = (source.Files ?? Array.Empty<Workspace.File>())
                .GroupBy(file => file.Name)
                .Select(fileGroup => MergeFiles(fileGroup.Key, fileGroup));

            var buffers = (source.Buffers ?? Array.Empty<Workspace.Buffer>())
                .GroupBy(buffer => buffer.Id)
                .SelectMany(bufferGroup => MergeBuffers(bufferGroup.Key, bufferGroup));


            var workspace = new Workspace(
                workspaceType: source.WorkspaceType,
                usings: source.Usings,
                files: files.ToArray(),
                buffers: buffers.ToArray());

            timeBudget?.RecordEntry(ProcessorName);

            return Task.FromResult(workspace);
        }

        private Workspace.File MergeFiles(string fileName, IEnumerable<Workspace.File> files)
        {
            var content = string.Empty;
            var order = 0;
            foreach (var file in files.OrderBy(file => file.Order))
            {
                order = file.Order;
                content = $"{content}{file.Text}{Padding}";

            }
            content = content.Substring(0, content.Length - Padding.Length);

            return new Workspace.File(fileName, content, order: order);
        }

        private IEnumerable<Workspace.Buffer> MergeBuffers(BufferId id, IEnumerable<Workspace.Buffer> buffers)
        {
            var position = 0;
            var content = string.Empty;
            var order = 0;

            Workspace.Buffer preRegion = null;
            Workspace.Buffer region = null;
            Workspace.Buffer postRegion = null;

            foreach (var buffer in buffers.OrderBy(buffer => buffer.Order))
            {
                order = buffer.Order;
                if (buffer.Position != 0)
                {
                    position = content.Length + buffer.Position;

                }
                content = $"{content}{buffer.Content}{Padding}";
            }

            content = content.Substring(0, content.Length - Padding.Length);

            region = new Workspace.Buffer(id, content, position: position, order: order);

            if (preRegion != null)
            {
                yield return preRegion;
            }

            if (region != null)
            {
                yield return region;
            }

            if (postRegion != null)
            {
                yield return postRegion;
            }
        }
    }
}