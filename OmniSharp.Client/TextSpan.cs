namespace OmniSharp.Client
{
    public class TextSpan
    {
        public TextSpan(
            int start = 0,
            int end = 0,
            int length = 0,
            bool isEmpty = false)
        {
            Start = start;
            End = end;
            Length = length;
            IsEmpty = isEmpty;
        }

        public int Start { get; }
        public int End { get; }
        public int Length { get; }
        public bool IsEmpty { get; }
    }
}