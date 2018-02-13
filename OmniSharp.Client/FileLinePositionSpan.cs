namespace OmniSharp.Client
{
    public class FileLinePositionSpan
    {
        public FileLinePositionSpan(
            string path = null,
            bool hasMappedPath = false,
            LinePosition startLinePosition = null,
            LinePosition endLinePosition = null,
            LinePositionSpan span = null)
        {
            Path = path;
            HasMappedPath = hasMappedPath;
            StartLinePosition = startLinePosition;
            EndLinePosition = endLinePosition;
            Span = span;
        }

        public string Path { get; }

        public bool HasMappedPath { get; }

        public LinePosition StartLinePosition { get; }

        public LinePosition EndLinePosition { get; }

        public LinePositionSpan Span { get; }

        public override string ToString() => $"{Path}: {Span}";
    }
}
