using System;
using Microsoft.CodeAnalysis.Text;

namespace WorkspaceServer
{
    public class SourceFile
    {
        public SourceText Text { get; }
        public bool HasSpan { get; }
        public TextSpan Span { get; }

        private SourceFile(SourceText text, TextSpan? span = null)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            HasSpan = span.HasValue;
            Span = span ?? default(TextSpan);

            if (HasSpan && (Span.Start > Text.Length || Span.End > Text.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(span));
            }
        }

        public static SourceFile Create(string text, TextSpan span)
            => new SourceFile(SourceText.From(text), span);

        public static SourceFile Create(string text, int position)
            => new SourceFile(SourceText.From(text), new TextSpan(position, 0));

        public static SourceFile Create(string text)
            => new SourceFile(SourceText.From(text));
    }
}
