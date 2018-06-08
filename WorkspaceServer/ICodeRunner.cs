using System.Threading.Tasks;
using Clockwise;
using WorkspaceServer.Models.Execution;

namespace WorkspaceServer
{
    public interface ICodeRunner
    {
        Task<RunResult> Run(Workspace workspace, Budget budget = null);
    }
}