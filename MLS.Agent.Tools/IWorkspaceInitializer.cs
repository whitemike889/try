using System.IO;
using System.Threading.Tasks;
using Clockwise;

namespace MLS.Agent.Tools
{
    public interface IWorkspaceInitializer
    {
        Task Initialize(
            DirectoryInfo directory,
            Budget budget = null);
    }
}
