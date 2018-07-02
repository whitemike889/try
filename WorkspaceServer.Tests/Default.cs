using System.Linq;
using System.Threading.Tasks;
using MLS.Agent.Tools;

namespace WorkspaceServer.Tests
{
    public static class Default
    {
        private static readonly WorkspaceRegistry _defaultWorkspaces = WorkspaceRegistry.CreateDefault();

        public static Task<Workspace> ConsoleWorkspace => _defaultWorkspaces.GetWorkspace("console");

        public static Task<Workspace> WebApiWorkspace => _defaultWorkspaces.GetWorkspace("aspnet.webapi");

        public static Task<Workspace> XunitWorkspace => _defaultWorkspaces.GetWorkspace("xunit");
    }
}
