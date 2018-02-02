using System.Threading.Tasks;
using WorkspaceServer.Models.Execution;

namespace WorkspaceServer
{
    public interface IWorksapceProcessor
    {
        Task<WorkspaceRunRequest> ProcessAsync(WorkspaceRunRequest source);
    }
}