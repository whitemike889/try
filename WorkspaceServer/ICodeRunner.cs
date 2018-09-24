using System.Threading.Tasks;
using Clockwise;
using MLS.Protocol;
using MLS.Protocol.Execution;

namespace WorkspaceServer
{
    public interface ICodeRunner
    {
        Task<RunResult> Run(WorkspaceRequest request, Budget budget = null);
    }
}