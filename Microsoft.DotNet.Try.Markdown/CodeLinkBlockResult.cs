using System.Collections.Generic;

namespace Microsoft.DotNet.Try.Markdown
{
    public class CodeLinkBlockResult
    {
        public CodeLinkBlockResult(
            string content = null,
            IList<string> errorMessages = null)
        {
            Content = content;
            ErrorMessages = errorMessages ?? new List<string>();
        }

        public string Content { get; }

        public IList<string> ErrorMessages { get; }
    }
}