using System.Collections.Generic;

namespace Microsoft.DotNet.Try.Markdown
{
    public sealed class FailedCodeBlockContentFetchResult : CodeBlockContentFetchResult
    {
        public FailedCodeBlockContentFetchResult(IList<string> errorMessages)
        {
            ErrorMessages = errorMessages;
        }

        public IList<string> ErrorMessages { get; }
    }
}