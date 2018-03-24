namespace OmniSharp.Client
{
    public static class LinePositionExtensions
    {
        public static LinePosition OneBased(this LinePosition position) =>
            position.IsOneBased
                ? position
                : new LinePosition(position.Line + 1, position.Character + 1, true);
    }
}
