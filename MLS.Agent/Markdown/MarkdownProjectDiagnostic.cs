using System;

namespace MLS.Agent.Markdown
{
    public class MarkdownProjectDiagnostic
    {
        public MarkdownProjectDiagnostic(
            string message,
            CodeLinkBlock codeLinkBlock)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("message", nameof(message));
            }

            Message = message;
            CodeLinkBlock = codeLinkBlock ?? throw new ArgumentNullException(nameof(codeLinkBlock));
        }

        public CodeLinkBlock CodeLinkBlock { get; }

        public string Message { get; }
    }
}