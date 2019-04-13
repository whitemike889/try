using System.Threading.Tasks;
using Clockwise;
using Microsoft.DotNet.Try.Protocol;
using Microsoft.DotNet.Try.Protocol.Completion;
using Microsoft.DotNet.Try.Protocol.Diagnostics;

namespace WorkspaceServer
{
    public interface ILanguageService
    {
        Task<CompletionResult> GetCompletionList(WorkspaceRequest request, Budget budget = null);
        Task<SignatureHelpResult> GetSignatureHelp(WorkspaceRequest request, Budget budget = null);
        Task<DiagnosticResult> GetDiagnostics(WorkspaceRequest request, Budget budget = null);
    }
}
