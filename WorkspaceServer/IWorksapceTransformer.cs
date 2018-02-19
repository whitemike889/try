using System.Threading.Tasks;
using Clockwise;
using WorkspaceServer.Models.Execution;

namespace WorkspaceServer
{
    public interface IWorksapceTransformer
    {
        Task<Workspace> TransformAsync(Workspace source, Budget timebudget = null);
    }
}