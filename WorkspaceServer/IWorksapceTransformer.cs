using System.Threading.Tasks;
using Clockwise;
using WorkspaceServer.Models.Execution;

namespace WorkspaceServer
{
    public interface IWorksapceTransformer
    {
        Task<Workspace> TransformAsync(Workspace source, TimeBudget timebudget = null);
    }
}