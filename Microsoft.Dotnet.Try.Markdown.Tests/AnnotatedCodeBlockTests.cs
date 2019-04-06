using System;
using FluentAssertions;
using Microsoft.DotNet.Try.Markdown;
using Xunit;

namespace Microsoft.Dotnet.Try.Markdown.Tests
{
    public class AnnotatedCodeBlockTests
    {
        [Fact]
        public void It_requires_options_to_initialize()
        {
            var block = new AnnotatedCodeBlock(); 

            block.Invoking(b => b.InitializeAsync().Wait())
                 .Should()
                 .Throw<InvalidOperationException>()
                 .And
                 .Message
                 .Should()
                 .Be("Attempted to initialize block before parsing code fence annotations");
        }
    }
}