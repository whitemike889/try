using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkspaceServer;

namespace MLS.Agent
{
    public class InstallCommand
    {
        public static async Task Do(string packageName, string packageSource)
        {
            

            var dotnet = new Dotnet();
            await dotnet.ToolInstall($"-g --add-source \"{packageSource}\" {packageName}");

        }
    }
}
