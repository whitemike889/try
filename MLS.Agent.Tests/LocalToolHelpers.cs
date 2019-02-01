using System.CommandLine;
using System.IO;
using System.Threading.Tasks;
using WorkspaceServer.Tests;

namespace MLS.Agent.Tests
{
    public static class LocalToolHelpers
    {
        public static async Task<DirectoryInfo> CreateTool(TestConsole console)
        {
            var asset = await Create.ConsoleWorkspaceCopy();
            await PackageCommand.Do(asset.Directory, console);
            return asset.Directory;
        }
    }
}
