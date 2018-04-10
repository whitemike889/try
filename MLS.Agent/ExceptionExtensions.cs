using System;
using System.Linq;
using Clockwise;
using static WorkspaceServer.Servers.WorkspaceServer;

namespace MLS.Agent
{
    public static class ExceptionExtensions
    {
        public static int ToHttpStatusCode(this Exception exception)
        {
            switch (exception)
            {
                case BudgetExceededException budgetExceededException:

                    var firstExceededEntry = budgetExceededException.Budget.Entries.FirstOrDefault(e => e.BudgetWasExceeded);

                    if (firstExceededEntry?.Name == UserCodeCompletedBudgetEntryName)
                    {
                        return 417;
                    }

                    return 504;

                default:
                    return 500;
            }
        }
    }
}
