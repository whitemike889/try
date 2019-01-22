using System.CommandLine;
using System.IO;
using System.Threading.Tasks;
using MLS.Agent.Tools;
using WorkspaceServer;
using WorkspaceServer.Packaging;

namespace MLS.Agent
{
    public static class InstallCommand
    {
        public static async Task Do(string packageName, DirectoryInfo addSource, IConsole console)
        {
            var dotnet = new Dotnet();
            var args = $"--add-source \"{addSource.FullName}\" {packageName} --tool-path {Package.DefaultPackagesDirectory}";
            console.Out.WriteLine($"Executing dotnet tool install {args}");
            (await dotnet.ToolInstall(args)).ThrowOnFailure();
        }
    }
}
