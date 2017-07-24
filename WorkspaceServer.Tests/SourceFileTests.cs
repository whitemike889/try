using System;
using FluentAssertions;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace WorkspaceServer.Tests
{
    public class SourceFileTests
    {
        [Fact]
        public void SourceFile_constructed_with_null_text_parameter_throws()
        {
            Action act = () => SourceFile.Create(text: null);

            act.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void HasSpan_returns_false_for_SourceFile_constructed_with_no_span()
        {
            SourceFile.Create(string.Empty).HasSpan.Should().BeFalse();
        }

        [Fact]
        public void HasSpan_returns_true_for_sourceFile_constructed_with_span()
        {
            var span = TextSpan.FromBounds(0, 0);
            SourceFile.Create(string.Empty, span).HasSpan.Should().BeTrue();
        }

        [Fact]
        public void SourceFile_constructed_with_span_that_starts_after_text_throws()
        {
            var span = TextSpan.FromBounds(1, 1);
            Action act = () => SourceFile.Create(string.Empty, span);

            act.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void SourceFile_constructed_with_span_that_ends_after_text_throws()
        {
            var span = TextSpan.FromBounds(0, 1);
            Action act = () => SourceFile.Create(string.Empty, span);

            act.ShouldThrow<ArgumentOutOfRangeException>();
        }
    }
}
