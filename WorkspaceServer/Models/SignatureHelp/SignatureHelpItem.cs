using System.Collections.Generic;

namespace WorkspaceServer.Models.SignatureHelp
{
    public class SignatureHelpItem
    {
        public string Name { get; set; }

        public string Label { get; set; }

        public string Documentation { get; set; }

        public IEnumerable<SignatureHelpParameter> Parameters { get; set; }
    }
}
