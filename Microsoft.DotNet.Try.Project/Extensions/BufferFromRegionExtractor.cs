using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Text;
using Buffer = MLS.Protocol.Execution.Workspace.Buffer;
using Workspace = MLS.Protocol.Execution.Workspace;

namespace Microsoft.DotNet.Try.Project.Extensions
{
    public class BufferFromRegionExtractor
    {
        public Workspace Extract(IReadOnlyCollection<Workspace.File> sourceFiles, string workspaceType = null, string[] usings = null)
        {
            var workSpaceType = workspaceType ?? "script";
            var (newFiles, newBuffers) = ProcessSourceFiles(sourceFiles);
            return new Workspace(files: newFiles, buffers: newBuffers, usings: usings, workspaceType: workSpaceType);
        }

        private static (Workspace.File[], Buffer[]) ProcessSourceFiles(IEnumerable<Workspace.File> sourceFiles)
        {
            var files = new Dictionary<string, Workspace.File>();
            var newBuffers = new List<Buffer>();
            foreach (var sourceFile in sourceFiles)
            {
                var buffers = SourceText.From(sourceFile.Text).ExtractBuffers(sourceFile.Name).ToList();
                if (buffers.Count > 0)
                {
                    files[sourceFile.Name] = sourceFile;
                    foreach (var buffer in buffers)
                    {
                        newBuffers.Add(buffer);
                    }
                }
                else
                {
                    newBuffers.Add(new Buffer(sourceFile.Name, sourceFile.Text, 0));
                }
            }
            return (files.Values.ToArray(), newBuffers.ToArray());
        }
    }
}
