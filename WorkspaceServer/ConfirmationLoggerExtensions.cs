using Clockwise;
using Pocket;
using WorkspaceServer.Models.Execution;

namespace WorkspaceServer
{
    internal static class ConfirmationLoggerExtensions
    {
        public static void Complete(
            this ConfirmationLogger logger,
            RunResult result,
            Budget budget)
        {
            if (result.WorkspaceServerException == null)
            {
                logger.Succeed("Completed with {budget}",
                               budget);
            }
            else
            {
                logger.Fail(
                    result.WorkspaceServerException,
                    "Completed with {budget}",
                    budget);
            }
        }
    }
}
