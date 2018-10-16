using System.IO;

namespace WorkspaceServer.Workspaces
{
    public interface IOutputConfiguration
    {
        FileInfo GetEntryPointAssemblyPath(DirectoryInfo directory, bool isWebProject);
        string GetTargetFramework(DirectoryInfo directory);
    }
}