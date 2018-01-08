using System.IO;

namespace OmniSharp.Client.Commands
{
    public class UpdateBuffer : AbstractOmniSharpFileCommandArguments
    {
        public UpdateBuffer(
            FileInfo fileName,
            string buffer) : base(fileName, buffer)
        {
        }

        public override string Command => CommandNames.UpdateBuffer;
    }
}
