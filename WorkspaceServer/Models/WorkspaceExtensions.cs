using System.Linq;
using WorkspaceServer.Models.Execution;

namespace WorkspaceServer.Models
{
    public static 

        class WorkspaceExtensions
    {
        public static Workspace.File GetFileFromActiveBuffer(this Workspace workspace, string activeBufferId)
        {
            var parsed = activeBufferId?.Split("@")[0].Trim();
            var fileName = string.IsNullOrWhiteSpace(parsed) ? "Program.cs" : parsed;
            var ret = workspace.Files.FirstOrDefault(f => f.Name == fileName);
            return ret;
        }

        public static int GetAbsolutePosiiton(this Workspace workspace, string activeBufferId, int relativePosition)
        {
            var aboslutePosition = workspace.Buffers.FirstOrDefault(b => b.Id == activeBufferId)?.Position ?? 0 + relativePosition;
            return aboslutePosition;
        }
    }
}