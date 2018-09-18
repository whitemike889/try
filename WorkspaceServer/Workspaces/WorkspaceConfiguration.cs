using System.Collections.Generic;

namespace WorkspaceServer.Workspaces
{
    public class WorkspaceConfiguration
    {
        public IReadOnlyCollection<string> CompilerArgs { get; set; }
    }
}