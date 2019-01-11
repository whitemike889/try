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
            string args = $"-g --add-source \"{packageSource}\" {packageName}";
            Console.WriteLine($"executing dotnet tool install {args}");
            var result = await dotnet.ToolInstall(args);
            Console.WriteLine(string.Join("\n", result.Output.Concat(result.Error)));


        }
    }
}
