using System.Threading.Tasks;

namespace LanguageServer
{
    public interface ILanguageServer
    {
        Task<ProcessResult> CompileAndExecute(BuildAndRunRequest request);
    }
}