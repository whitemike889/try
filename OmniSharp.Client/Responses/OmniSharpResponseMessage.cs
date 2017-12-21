namespace OmniSharp.Client.Responses
{
    public abstract class OmniSharpResponseMessage : OmniSharpMessage
    {
        protected OmniSharpResponseMessage(
            bool success,
            string message,
            string command,
            int seq,
            int requestSeq) : base("response", seq)
        {
            Success = success;
            Message = message;
            Command = command;
            Request_seq = requestSeq;
        }

        public string Command { get; }

        public bool Success { get; }

        public string Message { get; }

        public int Request_seq { get; }
    }
}
