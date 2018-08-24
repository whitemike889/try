using System;
using Clockwise;
using Microsoft.CodeAnalysis.Scripting;

namespace WorkspaceServer
{
    public static class ExceptionExtensions
    {
        public static string ToDisplayString(this Exception exception)
        {
            switch (exception)
            {
                case BudgetExceededException _:
                    return new TimeoutException().ToString();

                case CompilationErrorException _:
                    return null;

                default:
                    return exception?.ToString();
            }
        }

        public static bool IsConsideredRunFailure(this Exception exception) =>
            exception is TimeoutException ||
            exception is BudgetExceededException ||
            exception is CompilationErrorException;
    }
}
