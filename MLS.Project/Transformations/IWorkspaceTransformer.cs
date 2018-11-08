using System.Threading.Tasks;
using Clockwise;
using MLS.Protocol.Execution;

namespace MLS.Project.Transformations
{
    public interface IWorkspaceTransformer
    {
        Task<Workspace> TransformAsync(Workspace source, Budget timeBudget = null);
    }
}