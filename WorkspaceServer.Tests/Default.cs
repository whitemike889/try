using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MLS.Agent.Tools;

namespace WorkspaceServer.Tests
{
    public static class Default
    {
        private static readonly AsyncLazy<Workspace> _consoleWorkspace = new AsyncLazy<Workspace>(async () =>
        {
            var workspace = new Workspace(
                // To avoid collision with System.Console
                "TestTemplate.Console2",
                new DotnetWorkspaceInitializer("console", "test", async (directory, budget) =>
                 {
                     var dotnet = new Dotnet(directory);
                     await dotnet.AddPackage("Newtonsoft.Json", budget: budget);
                 }));

            await workspace.EnsureCreated();
            await workspace.EnsureBuilt();

            return workspace;
        });

        private static readonly AsyncLazy<Workspace> _webApiWorkspace = new AsyncLazy<Workspace>(async () =>
        {
            var workspace = new Workspace(
                "TestTemplate.WebApi",
                new DotnetWorkspaceInitializer(
                    "webapi",
                    "test",
                    afterCreate: async (directory, budget) =>
                    {
                        // the 2.1 template includes a forced HTTPS redirect that doesn't work without a cert installed, so we delete that line of code
                        var startupCs = directory.GetFiles("Startup.cs").Single();

                        string text = startupCs.Read();
                        text = text.Replace("app.UseHttpsRedirection();", "");
                        File.WriteAllText(startupCs.FullName, text);
                    }));

            await workspace.EnsureCreated();
            await workspace.EnsureBuilt();
            await workspace.EnsurePublished();

            return workspace;
        });

        public static Task<Workspace> ConsoleWorkspace => _consoleWorkspace.ValueAsync();

        public static Task<Workspace> WebApiWorkspace => _webApiWorkspace.ValueAsync();
    }
}
