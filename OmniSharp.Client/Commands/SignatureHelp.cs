using System.Collections.Generic;
using System.IO;

namespace OmniSharp.Client.Commands
{
    public class SignatureHelpRequest : AbstractOmniSharpFileCommandArguments
    {
        public SignatureHelpRequest(FileInfo fileName,string buffer, int line, int column, IEnumerable<LinePositionSpanTextChange> changes = null) : base(fileName, buffer, line, column, changes)
        {
        }

        public override string Command => CommandNames.SignatureHelp;
    }
}