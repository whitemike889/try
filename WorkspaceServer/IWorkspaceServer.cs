using System.Threading;
using System.Threading.Tasks;
using WorkspaceServer.Models.Completion;
using WorkspaceServer.Models.Execution;

namespace WorkspaceServer
{
    public interface IWorkspaceServer
    {
        Task<RunResult> Run(WorkspaceRunRequest request, CancellationToken? cancellationToken = null);

        Task<CompletionResult> GetCompletionList(CompletionRequest request);

        Task<DiagnosticResult> GetDiagnostics(WorkspaceRunRequest request);
    }
}
