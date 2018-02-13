using Newtonsoft.Json.Linq;

namespace OmniSharp.Client.Commands
{
    public class OmniSharpUnknownResponseMessage : OmniSharpResponseMessage
    {
        public OmniSharpUnknownResponseMessage(
            JToken body,
            bool success,
            string message,
            string command,
            int seq,
            int requestSeq) :
            base(success, message, command, seq, requestSeq)
        {
            Body = body;
        }

        public JToken Body { get; }
    }
}
