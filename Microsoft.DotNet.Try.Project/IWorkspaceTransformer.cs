using System.Threading.Tasks;
using Microsoft.DotNet.Try.Protocol;

namespace Microsoft.DotNet.Try.Project
{
    public interface IWorkspaceTransformer
    {
        Task<Workspace> TransformAsync(Workspace source);
    }
}