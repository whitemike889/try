using System;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MLS.Agent.Tools;
using WorkspaceServer;
using WorkspaceServer.Packaging;

namespace MLS.Agent
{
    public class InstallCommand
    {
        public static async Task Do(string packageName, DirectoryInfo addSource, IConsole console)
        {
            var dotnet = new Dotnet();
            (await dotnet.ToolInstall(
                packageName,
                Package.DefaultPackagesDirectory.FullName,
                addSource)).ThrowOnFailure();

        }
    }
}
