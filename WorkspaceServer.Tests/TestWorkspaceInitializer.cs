using System;
using System.IO;
using System.Threading.Tasks;
using Clockwise;
using WorkspaceServer.Workspaces;

namespace WorkspaceServer.Tests
{
    public class TestWorkspaceInitializer : WorkspaceInitializer
    {
        public int InitializeCount { get; private set; }

        public TestWorkspaceInitializer(
            string template, 
            string projectName, 
            Func<DirectoryInfo, Budget, Task> afterCreate = null) : 
            base(template, projectName, afterCreate)
        {
        }
        public override Task Initialize(DirectoryInfo directory, Budget budget = null)
        {
            InitializeCount++;
            return base.Initialize(directory, budget);
        }

    }
}
