using System;
using System.Collections.Generic;

namespace WorkspaceServer.Models.Execution
{
    public class RunResult
    {
        public RunResult(
            bool succeeded,
            IReadOnlyCollection<string> output,
            object returnValue = null,
            string exception = null,
            IReadOnlyCollection<Variable> variables = null)
        {
            Output = output ?? throw new ArgumentNullException(nameof(output));
            Succeeded = succeeded;
            Exception = exception;
            Variables = variables ?? Array.Empty<Variable>();
            ReturnValue = returnValue;
        }

        public bool Succeeded { get; }

        public IReadOnlyCollection<string> Output { get; }

        public object ReturnValue { get; }

        public IReadOnlyCollection<Variable> Variables { get; }

        public string Exception { get; }

        public override string ToString() =>
$@"Succeeded: {Succeeded}
ReturnValue: {ReturnValue}
Output: {string.Join("\n", Output)}";
    }
}