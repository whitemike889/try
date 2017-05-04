using System;
using System.Collections.Generic;

namespace WorkspaceServer
{
    public class ProcessResult
    {
        public ProcessResult(
            bool succeeded,
            IReadOnlyCollection<string> output,
            IReadOnlyCollection<LineInfo> lineInfo = null,
            object returnValue = null)
        {
            Output = output ?? throw new ArgumentNullException(nameof(output));
            Succeeded = succeeded;
            LineInfo = lineInfo ?? Array.Empty<LineInfo>();
            ReturnValue = returnValue;
        }

        public bool Succeeded { get; }

        public IReadOnlyCollection<string> Output { get; }

        public object ReturnValue { get; }

        public IReadOnlyCollection<LineInfo> LineInfo { get; }

        public override string ToString() =>
            $"Succeeded: {Succeeded}\n{string.Join("\n", Output)}";
    }

    public class LineInfo
    {
    }
}
