using WorkspaceServer;

namespace MLS.Agent
{
    public static class DefaultWorkspaces
    {
        public static WorkspaceServerRegistry CreateWorkspaceServerRegistry()
        {
            var registry = new WorkspaceServerRegistry();

            registry.AddWorkspace("console",
                                  workspace =>
                                  {
                                      workspace.CreateUsingDotnet("console");
                                      workspace.AddPackageReference("Microsoft.Net.Http");
                                      workspace.AddPackageReference("Newtonsoft.Json");
                                  });

            
            return registry;
        }
    }
}
