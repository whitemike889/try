using System.CommandLine;
using System.IO;
using System.Threading.Tasks;
using MLS.Agent.CommandLine;
using MLS.Agent.Tools;
using WorkspaceServer;
using WorkspaceServer.Packaging;

namespace MLS.Agent.CommandLine
{
    public static class InstallCommand
    {
        public static async Task Do(InstallOptions options, IConsole console)
        {
            var dotnet = new Dotnet();
            (await dotnet.ToolInstall(
                options.PackageName,
                Package.DefaultPackagesDirectory,
                options.AddSource)).ThrowOnFailure();

            var commandPath = Path.Combine(Package.DefaultPackagesDirectory.FullName, options.PackageName);
            (await MLS.Agent.Tools.CommandLine.Execute(commandPath, "extract-package")).ThrowOnFailure();
            (await MLS.Agent.Tools.CommandLine.Execute(commandPath, "prepare-package")).ThrowOnFailure();

        }
    }
}
