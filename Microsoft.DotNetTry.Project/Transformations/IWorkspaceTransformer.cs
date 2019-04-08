using System.Threading.Tasks;
using Clockwise;
using MLS.Protocol.Execution;

namespace Microsoft.DotNetTry.Project.Transformations
{
    public interface IWorkspaceTransformer
    {
        Task<Workspace> TransformAsync(Workspace source, Budget timeBudget = null);
    }
}