using System.Threading.Tasks;
using WorkspaceServer.Models.Completion;
using WorkspaceServer.Models.Execution;

namespace WorkspaceServer
{
    public interface IWorkspaceServer
    {
        Task<RunResult> Run(RunRequest request);

        Task<CompletionResult> GetCompletionList(CompletionRequest request);
    }
}