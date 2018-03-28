using WorkspaceServer.Models.Execution;

namespace WorkspaceServer.Models.Completion
{
    public class CompletionRequest : WorkspacePositionRequest
    {
        public CompletionRequest(Workspace workspace, string activeBufferId, int position) : base(workspace, activeBufferId, position)
        {
        }
    }
}
