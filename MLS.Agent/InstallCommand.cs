using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Threading.Tasks;
using WorkspaceServer;

namespace MLS.Agent
{
    public class InstallCommand
    {
        public static async Task Do(string packageName, string addSource, IConsole console)
        {
            var dotnet = new Dotnet();
            string args = $"-g --add-source \"{addSource}\" {packageName}";
            console.Out.WriteLine($"executing dotnet tool install {args}");
            var result = await dotnet.ToolInstall(args);
            console.Out.WriteLine(string.Join("\n", result.Output.Concat(result.Error)));
        }
    }
}
