using System.Linq;
using System.Threading.Tasks;
using MLS.Agent.Tools;

namespace WorkspaceServer.Tests
{
    public static class Default
    {
        private static readonly WorkspaceRegistry _defaultWorkspaces = WorkspaceRegistry.CreateDefault();

        public static Task<WorkspaceBuild> ConsoleWorkspace => _defaultWorkspaces.Get("console");

        public static Task<WorkspaceBuild> WebApiWorkspace => _defaultWorkspaces.Get("aspnet.webapi");

        public static Task<WorkspaceBuild> XunitWorkspace => _defaultWorkspaces.Get("xunit");
    }
}
