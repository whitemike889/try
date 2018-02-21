using System.Threading.Tasks;
using Clockwise;
using WorkspaceServer.Models.Completion;
using WorkspaceServer.Models.Execution;

namespace WorkspaceServer
{
    public interface IWorkspaceServer
    {
        Task<RunResult> Run(Workspace request, Budget budget = null);

        Task<CompletionResult> GetCompletionList(CompletionRequest request);

        Task<DiagnosticResult> GetDiagnostics(Workspace request);
    }
}
