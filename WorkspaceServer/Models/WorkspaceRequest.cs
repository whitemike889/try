using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using WorkspaceServer.Models.Execution;

namespace WorkspaceServer.Models
{
    public class WorkspaceRequest
    {
        public Workspace Workspace { get; }

        public HttpRequest HttpRequest { get; }

        public string ActiveBufferId { get; }

        [JsonIgnore]
        public int Position => Workspace.Buffers.SingleOrDefault(b => b.Id == ActiveBufferId)?.Position ?? 0;

        public WorkspaceRequest(Workspace workspace, HttpRequest httpRequest = null, string activeBufferId = null)

        {
            Workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));

            HttpRequest = httpRequest;
            ActiveBufferId = activeBufferId;
        }

        public static WorkspaceRequest FromDirectory(DirectoryInfo directory, string workspaceType)
        {
            var workspace = Workspace.FromDirectory(directory, workspaceType);

            return new WorkspaceRequest(workspace);
        }
    }
}
