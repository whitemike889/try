using System.Threading.Tasks;
using Clockwise;
using WorkspaceServer.Models;
using WorkspaceServer.Models.Completion;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Models.SingatureHelp;

namespace WorkspaceServer
{
    public interface ILanguageService
    {
        // FIX: (ILanguageService) consolidate input types on WorkspaceRequest
        Task<CompletionResult> GetCompletionList(WorkspaceRequest request, Budget budget = null);
        Task<DiagnosticResult> GetDiagnostics(Workspace request, Budget budget = null);
        Task<SignatureHelpResponse> GetSignatureHelp(WorkspaceRequest request, Budget budget = null);
    }
}
