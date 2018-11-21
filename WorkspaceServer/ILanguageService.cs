using System.Threading.Tasks;
using Clockwise;
using MLS.Protocol.Completion;
using MLS.Protocol.SignatureHelp;
using MLS.Protocol;
using MLS.Protocol.Diagnostics;

namespace WorkspaceServer
{
    public interface ILanguageService
    {
        Task<CompletionResult> GetCompletionList(WorkspaceRequest request, Budget budget = null);
        Task<SignatureHelpResult> GetSignatureHelp(WorkspaceRequest request, Budget budget = null);
        Task<DiagnosticResult> GetDiagnostics(WorkspaceRequest request, Budget budget = null);
    }
}
