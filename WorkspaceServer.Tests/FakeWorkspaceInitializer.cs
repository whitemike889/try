using System.IO;
using System.Threading.Tasks;
using Clockwise;
using MLS.Agent.Workspaces;

namespace WorkspaceServer.Tests
{
    public class FakeWorkspaceInitializer : IWorkspaceInitializer
    {
        public int InitializeCount { get; private set; }

        public Task Initialize(DirectoryInfo directory, Budget budget = null)
        {
            InitializeCount++;
            return Task.CompletedTask;
        }
    }
}
