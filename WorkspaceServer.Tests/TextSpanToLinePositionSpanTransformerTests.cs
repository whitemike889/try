using FluentAssertions;
using Microsoft.CodeAnalysis.Text;
using WorkspaceServer.Transformations;
using Xunit;

namespace WorkspaceServer.Tests
{
    public class TextSpanToLinePositionSpanTransformerTests
    {
        [Fact]
        public void It_Should_Convert_Text_Spans()
        {
            var sourceText = SourceText.From(
                CodeManipulation.EnforceLF("hello\nworld")
            );

            var span = new TextSpan(0, 11);
            var newSpan = TextSpanToLinePositionSpanTransformer.ToLinePositionSpan(span, sourceText);

            newSpan.Start.Line.Should().Be(0);
            newSpan.Start.Character.Should().Be(0);
            newSpan.End.Line.Should().Be(1);
            newSpan.End.Character.Should().Be(5);
        }
    }
}

