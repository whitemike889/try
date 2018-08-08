using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace WorkspaceServer.Transformations
{
    public static class TextSpanToLinePositionSpanTransformer
    {
        public static LinePositionSpan ToLinePositionSpan(this TextSpan span, SourceText text)
        {
            var line = text.Lines.GetLineFromPosition(span.Start);
            var col = span.Start - line.Start;

            var endLine = text.Lines.GetLineFromPosition(span.End);
            var endcol = span.End - endLine.Start;

            return new LinePositionSpan(
               new LinePosition(line.LineNumber, col),
               new LinePosition(endLine.LineNumber, endcol)
            );
        }
    }
}

