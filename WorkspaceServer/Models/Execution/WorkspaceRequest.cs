using System;

namespace WorkspaceServer.Models.Execution
{
    public class WorkspaceRequest
    {
        public Workspace Workspace { get; }
        public HttpRequest HttpRequest { get;  }

        public WorkspaceRequest(Workspace workspace, HttpRequest httpRequest = null)
        {
            Workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
            HttpRequest = httpRequest;
        }
    }
}