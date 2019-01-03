using System;
using System.IO;
using Xunit;
using FluentAssertions;

namespace MLS.Agent.Tests
{
    public class MarkdownFileTests
    {
        [Fact]
        public void Should_throw_exception_if_file_doesnt_exist()
        {
            string path = "DOESNOTEXIST";
            Action action = () => new MarkdownFile(new FileInfo(path));
            action.Should().Throw<FileNotFoundException>();
        }

        [Fact]
        public void Should_return_file_content()
        {
            var directoryInfo = TestAssets.BasicConsole;
            var file = new MarkdownFile(TestAssets.GetFileAtPath(directoryInfo, "Readme.md"));
            file.TryGetContent(out string content).Should().BeTrue();
            content.Should().Contain("*markdown file*");
        }

        [Fact]
        public void Should_throw_exception_if_file_is_null()
        {
            Action action = () => new MarkdownFile(null);
            action.Should().Throw<ArgumentNullException>();
        }
    }
}
