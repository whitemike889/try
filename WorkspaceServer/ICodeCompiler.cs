using System.Threading.Tasks;
using Clockwise;
using Microsoft.DotNet.Try.Protocol;

namespace WorkspaceServer
{
    public interface ICodeCompiler
    {
        Task<CompileResult> Compile(WorkspaceRequest request, Budget budget = null);
    }
}