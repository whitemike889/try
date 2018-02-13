using System;
using System.Threading.Tasks;
using MLS.Agent.Tools;

namespace WorkspaceServer.Tests
{
    internal static class Default
    {
        private static readonly AsyncLazy<Workspace> _templateWorkspace = new AsyncLazy<Workspace>(async () =>
        {
            var workspace = new Workspace("TestTemplate");
            await workspace.EnsureCreated();
            await workspace.EnsureBuilt();
            return workspace;
        });

        public static Task<Workspace> TemplateWorkspace => _templateWorkspace.ValueAsync();
    }
}
