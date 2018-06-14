using System;

namespace OmniSharp.Client
{
    public class LinePosition
    {
        public LinePosition(
            int line,
            int character,
            bool isOneBased = false)
        {
            var minValue = isOneBased
                               ? 1
                               : 0;

            if (line < minValue)
            {
                throw new ArgumentException($"Value must be at least {minValue} when {nameof(isOneBased)} is set to {isOneBased}", nameof(line));
            }

            if (character < minValue)
            {
                throw new ArgumentException($"Value must be at least {minValue} when {nameof(isOneBased)} is set to {isOneBased}", nameof(character));
            }

            Line = line;
            Character = character;
            IsOneBased = isOneBased;
        }

        public int Line { get; }

        public int Character { get; }

        public bool IsOneBased { get; }

        public override string ToString() => $"{Line},{Character}";
    }
}
