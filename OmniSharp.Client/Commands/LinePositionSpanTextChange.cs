namespace OmniSharp.Client.Commands
{
    public class LinePositionSpanTextChange
    {
        public string NewText { get; set; }

        public int StartLine { get; set; }

        public int StartColumn { get; set; }

        public int EndLine { get; set; }

        public int EndColumn { get; set; }
    }
}