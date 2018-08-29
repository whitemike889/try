using System.IO;
using System.Threading.Tasks;
using Clockwise;

namespace WorkspaceServer.Workspaces
{
    public interface IWorkspaceInitializer
    {
        Task Initialize(
            DirectoryInfo directory,
            Budget budget = null);
    }
}
