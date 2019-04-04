using System.Collections.Generic;

namespace Microsoft.DotNet.Try.Markdown
{
    public class CodeFenceOptionsParseResult
    {
        public static FailedCodeFenceOptionParseResult Failed(IList<string> errorMessages) => new FailedCodeFenceOptionParseResult(errorMessages);

        public static NoCodeFenceOptions None { get; } = new NoCodeFenceOptions();

        public static SuccessfulCodeFenceOptionParseResult Succeeded(CodeLinkBlockOptions options)
        {
            return new SuccessfulCodeFenceOptionParseResult(options);
        }
    }
}