namespace OmniSharp.Client
{
    public abstract class OmniSharpMessage
    {
        protected OmniSharpMessage(string type, int seq)
        {
            Type = type;
            Seq = seq;
        }

        public int Seq { get; }

        public string Type { get; }
    }
}