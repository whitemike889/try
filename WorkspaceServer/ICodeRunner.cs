using System.Threading.Tasks;
using Clockwise;
using WorkspaceServer.Models;
using WorkspaceServer.Models.Execution;

namespace WorkspaceServer
{
    public interface ICodeRunner
    {
        Task<RunResult> Run(WorkspaceRequest request, Budget budget = null);
    }
}