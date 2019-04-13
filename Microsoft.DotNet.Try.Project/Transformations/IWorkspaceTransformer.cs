using System.Threading.Tasks;
using Microsoft.DotNet.Try.Protocol.Execution;

namespace Microsoft.DotNet.Try.Project.Transformations
{
    public interface IWorkspaceTransformer
    {
        Task<Workspace> TransformAsync(Workspace source);
    }
}