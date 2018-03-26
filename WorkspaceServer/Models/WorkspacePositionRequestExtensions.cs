using System.Linq;

namespace WorkspaceServer.Models
{
    internal static class WorkspacePositionRequestExtensions
    {
        public static bool IsScriptCompatible(this WorkspacePositionRequest request)
        {
            var isScript = request.Workspace.WorkspaceType == "script";
            var hasSingleSourceCode = request.Workspace.Files?.Count == 1 || request.Workspace.Buffers?.Count == 1;
            return isScript || hasSingleSourceCode;
        }

        public static bool HasNullActiveBuffer(this WorkspacePositionRequest request)
        {
            return request.Workspace.Buffers.FirstOrDefault(b => b.Id == request.ActiveBufferId)?.Content == null;
        }

       
    }
}