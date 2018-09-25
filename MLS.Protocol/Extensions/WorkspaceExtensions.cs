using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MLS.Protocol.Execution;

namespace MLS.Protocol.Extensions
{
    public static class WorkspaceExtensions
    {
        public static IReadOnlyCollection<SourceFile> GetSourceFiles(this Workspace workspace)
        {
            return workspace.Files?.Select(f => SourceFile.Create(f.Text, f.Name)).ToArray() ?? Array.Empty<SourceFile>();
        }
    }
}
