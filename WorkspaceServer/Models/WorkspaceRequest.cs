using System;
using System.IO;
using WorkspaceServer.Models.Execution;

namespace WorkspaceServer.Models
{
    public class WorkspaceRequest
    {
        public Workspace Workspace { get; }
        /// <summary>
        /// Additional data for execution as http controller
        /// </summary>
        public HttpRequest HttpRequest { get; }
        /// <summary>
        /// Gets the is of the currently active buffer
        /// </summary>
        public string ActiveBufferId { get; }
        /// <summary>
        /// Gets the charet position in the <see cref="ActiveBufferId"/>
        /// </summary>
        public int Position { get; }

        public WorkspaceRequest(Workspace workspace, HttpRequest httpRequest = null, string activeBufferId = null, int position = 0)
        {
            Workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
            HttpRequest = httpRequest;
            ActiveBufferId = activeBufferId;
            Position = position;
        }

        public static WorkspaceRequest FromDirectory(DirectoryInfo directory, string workspaceType)
        {
            var workspace = Workspace.FromDirectory(directory, workspaceType);

            return new WorkspaceRequest(workspace);
        }
    }
}
