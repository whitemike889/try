using System.Threading.Tasks;
using Clockwise;
using MLS.Protocol;
using MLS.Protocol.Execution;

namespace WorkspaceServer
{
    public interface ICodeCompiler
    {
        Task<CompileResult> Compile(WorkspaceRequest request, Budget budget = null);
    }
}