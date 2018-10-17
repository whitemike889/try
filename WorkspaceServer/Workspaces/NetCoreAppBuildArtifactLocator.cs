using System.IO;
using System.Linq;

namespace WorkspaceServer.Workspaces
{
    public class NetCoreAppBuildArtifactLocator : IBuildArtifactLocator
    {
        public static readonly NetCoreAppBuildArtifactLocator Instance = new NetCoreAppBuildArtifactLocator();

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