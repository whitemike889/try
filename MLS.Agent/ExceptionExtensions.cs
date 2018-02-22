using System;
using System.Linq;
using Clockwise;
using WorkspaceServer.Servers.Dotnet;

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

                    if (firstExceededEntry?.Name == DotnetWorkspaceServer.UserCodeCompletedBudgetEntryName)
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
