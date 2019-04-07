using System.Collections.Generic;

namespace Microsoft.DotNet.Try.Markdown
{
    public abstract class CodeBlockContentFetchResult
    {
        internal CodeBlockContentFetchResult()
        {
        }

        public static CodeBlockContentFetchResult Failed(IList<string> errorMessages) => new FailedCodeBlockContentFetchResult(errorMessages);

        public static CodeBlockContentFetchResult None { get; } = new ExternalContentNotEnabledResult();

        public static CodeBlockContentFetchResult Succeeded(string content) => new SuccessfulCodeBlockContentFetchResult(content);
    }
}