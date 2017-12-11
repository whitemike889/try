using System.Linq;
using WorkspaceServer.Models.Execution;

namespace WorkspaceServer.Tests
{
    static class RunResultExtensions
    {
        public static RunResult WithExceptionStacktraceRemoved(this RunResult result)
        {
            var exception = result.Exception.Replace("\r\n", "\n").Split('\n').First();
            return new RunResult(result.Succeeded, result.Output, result.ReturnValue, exception, result.Variables);
        }
    }
}