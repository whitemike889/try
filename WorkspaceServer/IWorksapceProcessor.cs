using System.Threading.Tasks;
using Clockwise;
using WorkspaceServer.Models.Execution;

namespace WorkspaceServer
{
    public interface IWorksapceProcessor
    {
        Task<WorkspaceRunRequest> ProcessAsync(WorkspaceRunRequest source, TimeBudget timebudget = null);
    }
}