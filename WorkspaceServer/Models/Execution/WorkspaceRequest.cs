using System;
using System.IO;

namespace WorkspaceServer.Models.Execution
{
    public class WorkspaceRequest
    {
        public Workspace Workspace { get; }
        public HttpRequest HttpRequest { get; }

        public WorkspaceRequest(Workspace workspace, HttpRequest httpRequest = null)
        {
            Workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
            HttpRequest = httpRequest;
        }

        public static WorkspaceRequest FromDirectory(DirectoryInfo directory, string workspaceType)
        {
            var workspace = Workspace.FromDirectory(directory, workspaceType);

            return new WorkspaceRequest(workspace);
        }
    }
}
