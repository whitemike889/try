using System;

namespace WorkspaceServer.Models.Execution
{
    public class WorkspaceRequest
    {
        public Workspace Workspace { get; }
        public WebRequest WebRequest { get;  }

        public WorkspaceRequest(Workspace workspace, WebRequest webRequest = null)
        {
            Workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
            WebRequest = webRequest;
        }
    }
}