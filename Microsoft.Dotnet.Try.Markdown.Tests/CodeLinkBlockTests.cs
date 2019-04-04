using System;
using FluentAssertions;
using Markdig.Parsers;
using Microsoft.DotNet.Try.Markdown;
using Xunit;

namespace Microsoft.Dotnet.Try.Markdown.Tests
{
    public class CodeLinkBlockTests
    {
        [Fact]
        public void It_requires_options_to_initialize()
        {
            var block = new CodeLinkBlock(CreateParser(), 0);
            block.Invoking(b => b.InitializeAsync().Wait())
                 .Should().Throw<InvalidOperationException>("Attempted to initialize block before adding options");
        }

        protected virtual BlockParser CreateParser()
        {
            return null;//FIX new CodeLinkBlockParser(new CodeFenceOptionsParser());
        }
    }
}