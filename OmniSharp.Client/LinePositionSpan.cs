namespace OmniSharp.Client
{
    public class LinePositionSpan
    {
        public LinePositionSpan(LinePosition start, LinePosition end)
        {
            Start = start;
            End = end;
        }

        public LinePosition Start { get; }

        public LinePosition End { get; }

        public override string ToString() => $"({Start})-({End})";
    }
}