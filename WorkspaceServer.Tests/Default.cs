using System;
using System.Threading.Tasks;
using MLS.Agent.Tools;

namespace WorkspaceServer.Tests
{
    internal static class Default
    {
        private static readonly AsyncLazy<Workspace> _consoleWorkspace = new AsyncLazy<Workspace>(async () =>
        {
            var workspace = new Workspace(
                "TestTemplate.Console",
                new DotnetWorkspaceInitializer("console", "test"));

            await workspace.EnsureCreated();
            await workspace.EnsureBuilt();

            return workspace;
        });

        private static readonly AsyncLazy<Workspace> _webApiWorkspace = new AsyncLazy<Workspace>(async () =>
        {
            var workspace = new Workspace(
                "TestTemplate.WebApi",
                new DotnetWorkspaceInitializer("webapi", "test"));

            await workspace.EnsureCreated();
            await workspace.EnsureBuilt();

            return workspace;
        });

        public static Task<Workspace> ConsoleWorkspace => _consoleWorkspace.ValueAsync();

        public static Task<Workspace> WebApiWorkspace => _webApiWorkspace.ValueAsync();
    }
}
