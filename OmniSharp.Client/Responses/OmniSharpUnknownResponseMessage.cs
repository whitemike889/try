using Newtonsoft.Json.Linq;

namespace OmniSharp.Client.Responses
{
    public class OmniSharpUnknownResponseMessage : OmniSharpResponseMessage
    {
        public OmniSharpUnknownResponseMessage(
            JObject body,
            bool success,
            string message,
            string command,
            int seq,
            int requestSeq) :
            base(success, message, command, seq, requestSeq)
        {
            Body = body;
        }

        public JObject Body { get; }
    }
}
