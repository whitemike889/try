using System.Threading.Tasks;

namespace WorkspaceServer
{
    public interface IWorkspaceServer
    {
        Task<ProcessResult> CompileAndExecute(BuildAndRunRequest request);
    }
}