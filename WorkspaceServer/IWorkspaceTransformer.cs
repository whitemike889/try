using System.Threading.Tasks;
using Clockwise;
using MLS.Protocol.Execution;

namespace WorkspaceServer
{
    public interface IWorkspaceTransformer
    {
        Task<Workspace> TransformAsync(Workspace source, Budget timeBudget = null);
    }
}