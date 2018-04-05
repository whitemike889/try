using System.Collections.Generic;

namespace WorkspaceServer.Models.SingatureHelp
{
    public class SignatureHelpResponse
    {
        public IEnumerable<SignatureHelpItem> Signatures { get; set; }

        public int ActiveSignature { get; set; }

        public int ActiveParameter { get; set; }
    }
}