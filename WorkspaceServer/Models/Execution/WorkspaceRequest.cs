namespace WorkspaceServer.Models.Execution
{
    public class WorkspaceRequest
    {
        public Workspace Workspace { get; }

        public WorkspaceRequest(Workspace workspace)
        {
            Workspace = workspace;
        }
    }
}