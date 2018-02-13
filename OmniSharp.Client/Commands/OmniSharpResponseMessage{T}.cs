namespace OmniSharp.Client.Commands
{
    public class OmniSharpResponseMessage<T> : OmniSharpResponseMessage
    {
        public OmniSharpResponseMessage(
            T body,
            bool success,
            string message,
            string command,
            int seq,
            int requestSeq) :
            base(success, message, command, seq, requestSeq)
        {
            Body = body;
        }

        public T Body { get; }
    }
}
