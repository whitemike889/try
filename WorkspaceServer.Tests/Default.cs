using System;
using System.Diagnostics;
using System.Threading.Tasks;
using FluentAssertions;
using MLS.Agent.Tools;

namespace WorkspaceServer.Tests
{
    internal static class Default
    {
        private static readonly Lazy<Workspace> _templateWorkspace = new Lazy<Workspace>(() =>
        {
            var workspace = new Workspace("TestTemplate");
            Task.Run(() => workspace.EnsureCreated()).Wait();
            workspace.EnsureBuilt();
            return workspace;
        });

        public static Workspace TemplateWorkspace => _templateWorkspace.Value;

        public static TimeSpan Timeout() =>
            Debugger.IsAttached
                ? 10.Minutes()
                : 20.Seconds();
    }
}
