using System.Threading.Tasks;
using Clockwise;
using Microsoft.DotNet.Try.Protocol;
using Microsoft.DotNet.Try.Protocol.Execution;

namespace WorkspaceServer
{
    public interface ICodeCompiler
    {
        Task<CompileResult> Compile(WorkspaceRequest request, Budget budget = null);
    }
}