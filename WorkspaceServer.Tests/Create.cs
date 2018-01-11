using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MLS.Agent.Tools;

namespace WorkspaceServer.Tests
{
    internal static class Create
    {
        private static readonly Lazy<Workspace> _templateWorkspace = new Lazy<Workspace>(() =>
        {
            var workspace = new Workspace("TestTemplate");
            Task.Run(() => workspace.EnsureCreated()).Wait();
            workspace.EnsureBuilt();
            return workspace;
        });

        public static Workspace TestWorkspace([CallerMemberName] string testName = null) =>
            Workspace.Copy(_templateWorkspace.Value, testName);
    }
}
