using System;
using Microsoft.CodeAnalysis.Scripting;

namespace WorkspaceServer
{
    internal static class ExceptionExtensions
    {
        internal static string ToDisplayString(this Exception exception)
        {
            switch (exception)
            {
                case CompilationErrorException _:
                    return null;

                default:
                    return exception?.ToString();
            }
        }
    }
}