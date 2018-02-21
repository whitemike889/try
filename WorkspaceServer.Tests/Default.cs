using System;
using System.Threading.Tasks;
using MLS.Agent.Tools;
using Pocket;

namespace WorkspaceServer.Tests
{
    internal static class Default
    {
        private static readonly AsyncLazy<Workspace> _templateWorkspace = new AsyncLazy<Workspace>(async () =>
        {
            var workspace = new Workspace("TestTemplate");

            workspace.Directory.Refresh();

            if (!workspace.Directory.Exists)
            {
                Logger.Log.Info("Creating directory {directory}", workspace.Directory);
                workspace.Directory.Create();
            }

            return workspace;
        });

        public static Task<Workspace> TemplateWorkspace => _templateWorkspace.ValueAsync();
    }
}

