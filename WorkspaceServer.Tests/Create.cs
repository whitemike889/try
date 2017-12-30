using System;
using System.Runtime.CompilerServices;
using MLS.Agent.Tools;

namespace WorkspaceServer.Tests
{
    internal static class Create
    {
        public static Project TempProject(bool build = false, [CallerMemberName] string testName = null)
        {
            var project = new Project($"{DateTime.Now:yyyy-MM-dd--hh-mm-ss}.{testName}");

            project.EnsureCreated("console", build);

            return project;
        }
    }
}
