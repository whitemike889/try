using System.CommandLine;
using System.IO;
using System.Threading.Tasks;
using MLS.Agent.Tools;
using WorkspaceServer;
using WorkspaceServer.WorkspaceFeatures;

namespace MLS.Agent.CommandLine
{
    public static class InstallCommand
    {
        public static async Task Do(InstallOptions options, IConsole console)
        {
            var dotnet = new Dotnet();
            (await dotnet.ToolInstall(
                options.PackageName,
                options.Location,
                options.AddSource)).ThrowOnFailure();

            var tool = new WorkspaceServer.WorkspaceFeatures.PackageTool(options.PackageName, options.Location);
            await tool.Prepare();
        }
    }
}
