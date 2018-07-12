using System.Threading.Tasks;
using Clockwise;
using WorkspaceServer.Models;
using WorkspaceServer.Models.Completion;
using WorkspaceServer.Models.SingatureHelp;

namespace WorkspaceServer
{
    public interface ILanguageService
    {
        Task<CompletionResult> GetCompletionList(WorkspaceRequest request, Budget budget = null);
        Task<SignatureHelpResponse> GetSignatureHelp(WorkspaceRequest request, Budget budget = null);
    }
}
