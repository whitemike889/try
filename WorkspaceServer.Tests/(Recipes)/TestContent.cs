using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace TestUtility
{
    /// <summary>
    /// MarkupCode allows encoding additional pieces of information along with a piece of source code
    /// that are useful for testing. The following information can be encoded:
    /// 
    /// $$ - The position in the code. There can be no more than one of these.
    /// 
    /// [| ... |] - A span in the code. There can be many of these and they can be nested.
    /// 
    /// {|Name: ... |} - A span of code that is annotated with a name. There can be many of these and
    /// they can be nested.
    /// 
    /// This is similar the MarkupTestFile used in Roslyn:
    ///     https://github.com/dotnet/roslyn/blob/master/src/Test/Utilities/Shared/MarkedSource/MarkupTestFile.cs
    /// </summary>
    public class TestContent
    {
        private int? position;
        private ImmutableDictionary<string, ImmutableList<TextSpan>> spans;
        private SourceText text;

        public string Code { get; }
        public SourceText Text => GetOrCreateText();

        public int Position => this.position.Value;
        public bool HasPosition => this.position.HasValue;

        public TextPoint GetPointFromPosition()
        {
            return this.Text.GetPointFromPosition(this.Position);
        }

        public TextRange GetRangeFromSpan(TextSpan span)
        {
            return this.Text.GetRangeFromSpan(span);
        }

        private TestContent(string code, int? position, ImmutableDictionary<string, ImmutableList<TextSpan>> spans)
        {
            this.Code = code;
            this.position = position;
            this.spans = spans;
        }

        private SourceText GetOrCreateText()
        {
            if (this.text == null)
            {
                this.text = SourceText.From(this.Code);
            }

            return this.text;
        }

        public ImmutableList<TextSpan> GetSpans(string name = null)
        {
            if (this.spans.TryGetValue(name ?? string.Empty, out var result))
            {
                return result;
            }

            return ImmutableList<TextSpan>.Empty;
        }

        public static TestContent Parse(string input)
        {
            var markupLength = input.Length;
            var codeBuilder = new StringBuilder(markupLength);

            int? position = null;
            var spanStartStack = new Stack<int>();
            var namedSpanStartStack = new Stack<Tuple<int, string>>();
            var spans = new Dictionary<string, List<TextSpan>>();

            var codeIndex = 0;
            var markupIndex = 0;

            while (markupIndex < markupLength)
            {
                var ch = input[markupIndex];

                switch (ch)
                {
                    case '$':
                        if (position == null &&
                            markupIndex + 1 < markupLength &&
                            input[markupIndex + 1] == '$')
                        {
                            position = codeIndex;
                            markupIndex += 2;
                            continue;
                        }

                        break;

                    case '[':
                        if (markupIndex + 1 < markupLength &&
                            input[markupIndex + 1] == '|')
                        {
                            spanStartStack.Push(codeIndex);
                            markupIndex += 2;
                            continue;
                        }

                        break;

                    case '{':
                        if (markupIndex + 1 < markupLength &&
                            input[markupIndex + 1] == '|')
                        {
                            var nameIndex = markupIndex + 2;
                            var nameStartIndex = nameIndex;
                            var nameLength = 0;
                            var found = false;

                            // Parse out name
                            while (nameIndex < markupLength)
                            {
                                if (input[nameIndex] == ':')
                                {
                                    found = true;
                                    break;
                                }

                                nameLength++;
                                nameIndex++;
                            }

                            if (found)
                            {
                                var name = input.Substring(nameStartIndex, nameLength);
                                namedSpanStartStack.Push(Tuple.Create(codeIndex, name));
                                markupIndex = nameIndex + 1; // Move after ':'
                                continue;
                            }

                            // We didn't find a ':'. In this case, we just carry on...
                        }

                        break;

                    case '|':
                        if (markupIndex + 1 < markupLength)
                        {
                            if (input[markupIndex + 1] == ']')
                            {
                                if (spanStartStack.Count == 0)
                                {
                                    throw new ArgumentException("Saw |] without matching [|");
                                }

                                var spanStart = spanStartStack.Pop();

                                AddSpan(spans, string.Empty, spanStart, codeIndex);
                                markupIndex += 2;

                                continue;
                            }

                            if (input[markupIndex + 1] == '}')
                            {
                                if (namedSpanStartStack.Count == 0)
                                {
                                    throw new ArgumentException("Saw |} without matching {|");
                                }

                                var tuple = namedSpanStartStack.Pop();
                                var spanStart = tuple.Item1;
                                var spanName = tuple.Item2;

                                AddSpan(spans, spanName, spanStart, codeIndex);
                                markupIndex += 2;

                                continue;
                            }
                        }

                        break;
                }

                codeBuilder.Append(ch);
                codeIndex++;
                markupIndex++;
            }

            var finalSpans = spans.ToImmutableDictionary(
                keySelector: kvp => kvp.Key,
                elementSelector: kvp => kvp.Value.ToImmutableList().Sort());

            return new TestContent(codeBuilder.ToString(), position, finalSpans);
        }

        private static void AddSpan(Dictionary<string, List<TextSpan>> spans, string spanName, int spanStart, int spanEnd)
        {
            if (!spans.TryGetValue(spanName, out var spanList))
            {
                spanList = new List<TextSpan>();
                spans.Add(spanName, spanList);
            }

            spanList.Add(TextSpan.FromBounds(spanStart, spanEnd));
        }
    }

    public struct TextPoint : IEquatable<TextPoint>, IComparable<TextPoint>
    {
        public readonly int Line;

        // NOTE: This is intentionally called offset. Traditionally, "columns" are computed after tabs are expanded.
        public readonly int Offset;

        public TextPoint(int line, int offset)
        {
            this.Line = line;
            this.Offset = offset;
        }

        public int CompareTo(TextPoint other)
        {
            if (this.Line < other.Line)
            {
                return -1;
            }
            else if (this.Line > other.Line)
            {
                return 1;
            }
            else if (this.Offset < other.Offset)
            {
                return -1;
            }
            else if (this.Offset > other.Offset)
            {
                return 1;
            }

            return 0;
        }

        public override bool Equals(object obj)
            => obj is TextPoint && Equals((TextPoint) obj);

        public bool Equals(TextPoint other)
            => this.Line == other.Line && this.Offset == other.Line;

        public override int GetHashCode()
            => this.Line ^ this.Offset;

        public override string ToString()
            => $"{{Line={this.Line}, Offset={this.Offset}}}";

        public static bool operator ==(TextPoint point1, TextPoint point2)
            => point1.Equals(point2);

        public static bool operator !=(TextPoint point1, TextPoint point2)
            => !point1.Equals(point2);

        public static bool operator <(TextPoint point1, TextPoint point2)
            => point1.CompareTo(point2) < 0;

        public static bool operator <=(TextPoint point1, TextPoint point2)
            => point1.CompareTo(point2) <= 0;

        public static bool operator >(TextPoint point1, TextPoint point2)
            => point1.CompareTo(point2) > 0;

        public static bool operator >=(TextPoint point1, TextPoint point2)
            => point1.CompareTo(point2) >= 0;
    }

    public struct TextRange : IEquatable<TextRange>, IComparable<TextRange>
    {
        public readonly TextPoint Start;
        public readonly TextPoint End;

        public bool IsEmpty => this.Start == this.End;

        public TextRange(TextPoint start, TextPoint end)
        {
            this.Start = start;
            this.End = end;
        }

        public int CompareTo(TextRange other)
        {
            if (this.Start < other.Start)
            {
                return -1;
            }
            else if (this.Start > other.Start)
            {
                return 1;
            }
            else if (this.End < other.End)
            {
                return -1;
            }
            else if (this.End > other.End)
            {
                return 1;
            }

            return 0;
        }

        public override bool Equals(object obj)
            => obj is TextRange && Equals((TextRange) obj);

        public bool Equals(TextRange other)
            => this.Start == other.Start && this.End == other.End;

        public override int GetHashCode()
            => this.Start.GetHashCode() ^ this.End.GetHashCode();

        public override string ToString()
            => $"{{Start={this.Start}, End={this.End}}}";

        public static bool operator ==(TextRange range11, TextRange range12)
            => range11.Equals(range12);

        public static bool operator !=(TextRange range11, TextRange range12)
            => !range11.Equals(range12);

        public static bool operator <(TextRange range11, TextRange range12)
            => range11.CompareTo(range12) < 0;

        public static bool operator <=(TextRange range11, TextRange range12)
            => range11.CompareTo(range12) <= 0;

        public static bool operator >(TextRange range11, TextRange range12)
            => range11.CompareTo(range12) > 0;

        public static bool operator >=(TextRange range11, TextRange range12)
            => range11.CompareTo(range12) >= 0;
    }

    public static class SourceTextExtensions
    {
        public static TextPoint GetPointFromPosition(this SourceText text, int position)
        {
            var line = text.Lines.GetLineFromPosition(position);
            var offset = position - line.Start;

            return new TextPoint(line.LineNumber, offset);
        }

        public static TextRange GetRangeFromSpan(this SourceText text, TextSpan span)
        {
            var startLine = text.Lines.GetLineFromPosition(span.Start);
            var startOffset = span.Start - startLine.Start;
            var endLine = text.Lines.GetLineFromPosition(span.End);
            var endOffset = span.End - endLine.Start;

            return new TextRange(
                start: new TextPoint(startLine.LineNumber, startOffset),
                end: new TextPoint(endLine.LineNumber, endOffset));
        }
    }
}
