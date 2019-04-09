using System.Collections.Generic;

namespace Microsoft.DotNet.Try.Protocol.SignatureHelp
{
    public class SignatureHelpItem
    {
        public string Name { get; set; }

        public string Label { get; set; }

        public MarkdownString Documentation { get; set; }

        public IEnumerable<SignatureHelpParameter> Parameters { get; set; }
    }
}
