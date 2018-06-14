using WorkspaceServer;

namespace MLS.Agent
{
    public static class DefaultWorkspaces
    {
        public static WorkspaceRegistry CreateWorkspaceServerRegistry()
        {
            var registry = new WorkspaceRegistry();

            registry.AddWorkspace("console",
                                  workspace =>
                                  {
                                      workspace.CreateUsingDotnet("console");
                                      workspace.AddPackageReference("Newtonsoft.Json");
                                  });

            registry.AddWorkspace("nodatime.api",
                                  workspace =>
                                  {
                                      workspace.CreateUsingDotnet("console");
                                      workspace.AddPackageReference("NodaTime", "2.3.0");
                                      workspace.AddPackageReference("NodaTime.Testing", "2.3.0");
                                  });

            registry.AddWorkspace("aspnet.webapi",
                                  workspace =>
                                  {
                                      workspace.CreateUsingDotnet("webapi");
                                      workspace.RequiresPublish = true;
                                  });

            return registry;
        }
    }
}
