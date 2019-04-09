using System;
using System.IO;
using Microsoft.DotNet.Try.Protocol;
using WorkspaceServer.Models.Execution;

namespace WorkspaceServer.Models
{
    public static class WorkspaceRequestFactory
    { 

        public static WorkspaceRequest CreateRequestFromDirectory(DirectoryInfo directory, string workspaceType)
        {
            var workspace = WorkspaceFactory.CreateWorkspaceFromDirectory(directory, workspaceType);

            return new WorkspaceRequest(workspace);
        }
    }
}
