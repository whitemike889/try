using WorkspaceServer.Models.Execution;

namespace WorkspaceServer.Models
{
    public class WorkspacePositionRequest
    {
        public WorkspacePositionRequest(Workspace workspace, string activeBufferId, int position)
        {
            Position = position;
            ActiveBufferId = activeBufferId ?? string.Empty;
            Workspace = workspace;
        }

        public Workspace Workspace { get; }

        public string ActiveBufferId { get; }
        public int Position { get; }
    }
}