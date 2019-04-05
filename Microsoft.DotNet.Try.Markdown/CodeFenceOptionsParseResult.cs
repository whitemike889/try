using System.Collections.Generic;

namespace Microsoft.DotNet.Try.Markdown
{
    public abstract class CodeFenceOptionsParseResult
    {
        internal CodeFenceOptionsParseResult()
        {
        }

        public static CodeFenceOptionsParseResult Failed(IList<string> errorMessages) => new FailedCodeFenceOptionParseResult(errorMessages);

        public static CodeFenceOptionsParseResult None { get; } = new NoCodeFenceOptions();

        public static CodeFenceOptionsParseResult Succeeded(CodeLinkBlockOptions options) => new SuccessfulCodeFenceOptionParseResult(options);
    }
}