using System.Collections.Generic;

namespace Microsoft.DotNet.Try.Markdown
{
    public sealed class FailedCodeFenceOptionParseResult : CodeFenceOptionsParseResult
    {
        public FailedCodeFenceOptionParseResult(IList<string> errorMessages)
        {
            ErrorMessages = errorMessages;
        }

        public IList<string> ErrorMessages { get; }
    }
}