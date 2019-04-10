using System.CommandLine;
using System.IO;
using System.Threading.Tasks;
using MLS.Agent.Tools;
using WorkspaceServer;

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

            var commandPath = Path.Combine(options.Location.FullName, options.PackageName);

            (await Tools.CommandLine.Execute(commandPath, "extract-package")).ThrowOnFailure();
            (await Tools.CommandLine.Execute(commandPath, "prepare-package")).ThrowOnFailure();
        }
    }
}
