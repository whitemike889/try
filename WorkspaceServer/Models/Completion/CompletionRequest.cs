namespace WorkspaceServer.Models.Completion
{
    public class CompletionRequest
    {
        public CompletionRequest(string source, int position)
        {
            RawSource = source ?? string.Empty;
            Position = position;
        }

        public string RawSource { get; }
        public int Position { get; }
    }
}
