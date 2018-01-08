using System;
using System.Runtime.CompilerServices;
using MLS.Agent.Tools;

namespace WorkspaceServer.Tests
{
    internal static class Create
    {
        private static readonly Lazy<Project> _templateProject = new Lazy<Project>(() =>
        {
            var project = new Project("TestTemplate");
            project.EnsureCreated("console");
            project.EnsureBuilt();
            return project;
        });

        public static Project TempProject([CallerMemberName] string testName = null) =>
            Project.Copy(_templateProject.Value, testName);
    }
}
