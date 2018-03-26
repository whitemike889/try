using System;
using System.Threading.Tasks;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Transformations;

namespace WorkspaceServer.Models
{
    public class WorkspacePositionRequest
    {
        public WorkspacePositionRequest(Workspace workspace, string activeBufferId, int position, string[] implicitUsings = null)
        {
            ImplicitUsings = implicitUsings ?? Array.Empty<string>();
            Position = position;
            ActiveBufferId = activeBufferId ?? string.Empty;
            Workspace = workspace;
        }

        public Workspace Workspace { get; }

        public string ActiveBufferId { get; }
        public int Position { get; }
        public string[] ImplicitUsings { get; }
    }
}