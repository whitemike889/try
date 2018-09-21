using System;
using System.IO;
using MLS.Protocol;
using MLS.Protocol.Execution;
using WorkspaceServer.Models.Execution;

namespace WorkspaceServer.Models
{
    public static class WorkspaceRequestExtensions
    { 

        public static WorkspaceRequest FromDirectory(DirectoryInfo directory, string workspaceType)
        {
            var workspace = WorkspaceFactory.FromDirectory(directory, workspaceType);

            return new WorkspaceRequest(workspace);
        }
    }
}
