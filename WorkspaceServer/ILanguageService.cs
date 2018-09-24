using System.Threading.Tasks;
using Clockwise;
using WorkspaceServer.Models;
using MLS.Protocol.Completion;
using MLS.Protocol.SignatureHelp;
using MLS.Protocol;

namespace WorkspaceServer
{
    public interface ILanguageService
    {
        Task<CompletionResult> GetCompletionList(WorkspaceRequest request, Budget budget = null);
        Task<SignatureHelpResult> GetSignatureHelp(WorkspaceRequest request, Budget budget = null);
    }
}
