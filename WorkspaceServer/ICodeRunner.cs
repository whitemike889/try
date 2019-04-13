using System.Threading.Tasks;
using Clockwise;
using Microsoft.DotNet.Try.Protocol;

namespace WorkspaceServer
{
    public interface ICodeRunner
    {
        Task<RunResult> Run(WorkspaceRequest request, Budget budget = null);
    }
}