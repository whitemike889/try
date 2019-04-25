using System.IO;
using System.Threading.Tasks;
using Clockwise;

namespace WorkspaceServer.Packaging
{
    public interface IToolPackageLocator
    {
        Task<DirectoryInfo> PrepareToolAndLocateAssetDirectory(FileInfo tool, Budget budget = null);
    }
}