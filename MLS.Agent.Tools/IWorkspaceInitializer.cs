using System.IO;
using System.Threading.Tasks;

namespace MLS.Agent.Tools
{
    public interface IWorkspaceInitializer
    {
        Task Initialize(DirectoryInfo directory);
    }
}