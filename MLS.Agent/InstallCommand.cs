using System;
using System.CommandLine;
using System.Linq;
using System.Threading.Tasks;
using WorkspaceServer;
using WorkspaceServer.Packaging;

namespace MLS.Agent
{
    public class InstallCommand
    {
        public static async Task Do(string packageName, string addSource, IConsole console)
        {
            var dotnet = new Dotnet();
            string args = $" --add-source \"{addSource}\" {packageName} --tool-path {Package.DefaultPackagesDirectory}";
            console.Out.WriteLine($"executing dotnet tool install {args}");
            var result = await dotnet.ToolInstall(args);
            if (result.ExitCode != 0)
            {
                throw new Exception($"Installation failed with error {result.Error}");
            }
        }
    }
}
