namespace MLS.Agent.Tools
{
    public static class ProjectExtensions
    {
        public static void EnsureCreated(this Project project, string template, bool build = false)
        {
            if (!project.Directory.Exists)
            {
                project.Directory.Create();
            }

            if (project.Directory.GetFiles().Length == 0)
            {
                var dotnet = new Dotnet(project.Directory);
                dotnet.New(template).ThrowOnFailure();

                if (build)
                {
                    dotnet.Build().ThrowOnFailure();
                }
            }
        }
    }
}
