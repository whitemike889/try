using System.Threading.Tasks;
using Clockwise;
using MLS.Protocol.Execution;

namespace MLS.Protocol.Execution
{
    public interface IWorkspaceTransformer
    {
        Task<Workspace> TransformAsync(Workspace source, Budget timeBudget = null);
    }
}