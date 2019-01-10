using System.IO;
using System.Threading.Tasks;
using Clockwise;

namespace WorkspaceServer.Packaging
{
    public interface IPackageInitializer
    {
        Task Initialize(
            DirectoryInfo directory,
            Budget budget = null);
    }
}
