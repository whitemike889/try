using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.DotNet.Try.Project.Execution;
using Microsoft.DotNet.Try.Protocol.Execution;

namespace Microsoft.DotNet.Try.Project.Extensions
{
    public static class WorkspaceExtensions
    {
        public static IReadOnlyCollection<SourceFile> GetSourceFiles(this Workspace workspace)
        {
            return workspace.Files?.Select(f => f.ToSourceFile()).ToArray() ?? Array.Empty<SourceFile>();
        }

        public static IEnumerable<Viewport> ExtractViewPorts(this Workspace ws)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            foreach (var file in ws.Files)
            {
                foreach (var viewPort in file.ExtractViewPorts())
                {
                    yield return viewPort;
                }
            }
        }
    }
}
