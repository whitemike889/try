using System.CommandLine;
using System.IO;
using System.Threading.Tasks;
using MLS.Agent.CommandLine;
using MLS.Agent.Tools;
using WorkspaceServer;
using WorkspaceServer.Packaging;

namespace MLS.Agent
{
    public static class InstallCommand
    {
        public static async Task Do(InstallOptions options, IConsole console)
        {
            var dotnet = new Dotnet();
            (await dotnet.ToolInstall(
                options.PackageName,
                Package.DefaultPackagesDirectory.FullName,
                options.AddSource)).ThrowOnFailure();

        }
    }
}
