using System;
using System.IO;

namespace OmniSharp.Client.Commands
{
    public class CodeCheck : AbstractOmniSharpFileCommandArguments
    {
        public CodeCheck(FileInfo fileName = null, string buffer = null) : base(fileName, buffer)
        {
        }

        public override string Command => CommandNames.CodeCheck;
    }
}
