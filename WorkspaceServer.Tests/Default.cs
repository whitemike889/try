using System.Threading.Tasks;
using WorkspaceServer.Workspaces;

namespace WorkspaceServer.Tests
{
    public static class Default
    {
        private static readonly WorkspaceRegistry _defaultWorkspaces = WorkspaceRegistry.CreateForHostedMode();

        public static Task<WorkspaceBuild> ConsoleWorkspace => _defaultWorkspaces.Get("console");

        public static Task<WorkspaceBuild> WebApiWorkspace => _defaultWorkspaces.Get("aspnet.webapi");

        public static Task<WorkspaceBuild> XunitWorkspace => _defaultWorkspaces.Get("xunit");
        public static Task<WorkspaceBuild> NetstandardWorkspace => _defaultWorkspaces.Get("blazor-console");
    }
}
