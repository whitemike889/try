using System.Threading.Tasks;
using Clockwise;
using WorkspaceServer.Models.Completion;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Models.SingatureHelp;

namespace WorkspaceServer
{
    public interface IWorkspaceServer
    {
        Task<RunResult> Run(Workspace workspace, Budget budget = null);

        Task<CompletionResult> GetCompletionList(CompletionRequest request, Budget budget = null);

        Task<DiagnosticResult> GetDiagnostics(Workspace request, Budget budget = null);
        Task<SignatureHelpResponse> GetSignatureHelp(SignatureHelpRequest request, Budget budget = null);
    }
}
