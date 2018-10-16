using System.IO;

namespace WorkspaceServer.Workspaces
{
    public class NetstandardTargetConfiguration : IOutputConfiguration
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
}