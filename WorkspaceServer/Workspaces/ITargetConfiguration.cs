using System.IO;
using System.Linq;

namespace WorkspaceServer.Workspaces
{
    public interface ITargetConfiguration
    {
        FileInfo GetEntryPointAssemblyPath(DirectoryInfo directory, bool isWebProject);

        string GetTargetFramework(DirectoryInfo directory);
    }

    public class NetstandardTargetConfiguration : ITargetConfiguration
    {
        public FileInfo GetEntryPointAssemblyPath(DirectoryInfo directory, bool isWebProject)
        {
            return null;
        }

        public string GetTargetFramework(DirectoryInfo directory)
        {
            return "netstandard2.0";
        }
    }

    public class ConsoleTargetConfiguration : ITargetConfiguration
    {
        public static readonly ConsoleTargetConfiguration Instance = new ConsoleTargetConfiguration();

        public FileInfo GetEntryPointAssemblyPath(DirectoryInfo directory, bool isWebProject)
        {
            var depsFile = directory.GetFiles("*.deps.json", SearchOption.AllDirectories).First();

            var entryPointAssemblyName = DepsFileParser.GetEntryPointAssemblyName(depsFile);

            var path =
                Path.Combine(
                    directory.FullName,
                    "bin",
                    "Debug",
                    GetTargetFramework(directory));

            if (isWebProject)
            {
                path = Path.Combine(path, "publish");
            }

            return new FileInfo(Path.Combine(path, entryPointAssemblyName));
        }

        public string GetTargetFramework(DirectoryInfo directory)
        {
            var runtimeConfig = directory.GetFiles("*.runtimeconfig.json", SearchOption.AllDirectories).First();
            return  RuntimeConfig.GetTargetFramework(runtimeConfig);
        }
    }
}