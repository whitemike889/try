using System.Threading.Tasks;
using Clockwise;
using WorkspaceServer.Models.Execution;

namespace WorkspaceServer
{
    public interface IWorkspaceTransformer
    {
        Task<Workspace> TransformAsync(Workspace source, Budget timeBudget = null);
    }
}