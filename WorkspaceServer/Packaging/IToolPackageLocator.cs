using System.IO;
using System.Threading.Tasks;
using Clockwise;
using WorkspaceServer.WorkspaceFeatures;

namespace WorkspaceServer.Packaging
{
    public interface IToolPackageLocator
    {
        Task<DirectoryInfo> PrepareToolAndLocateAssetDirectory(PackageTool tool);
    }
}