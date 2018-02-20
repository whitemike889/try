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
            Budget budget) =>
            logger.Succeed("Completed with result {succeeded} and {budget}",
                           result.Succeeded,
                           budget);
    }
}
