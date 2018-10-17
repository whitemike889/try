using System.IO;

namespace WorkspaceServer.Workspaces
{
    public interface IBuildArtifactLocator
    {
        FileInfo GetEntryPointAssemblyPath(DirectoryInfo directory, bool isWebProject);
        string GetTargetFramework(DirectoryInfo directory);
    }
}