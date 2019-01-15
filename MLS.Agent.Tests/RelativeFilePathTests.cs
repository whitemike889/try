using FluentAssertions;
using System;
using Xunit;
namespace MLS.Agent.Tests
{
    public class RelativeFilePathTests
    {
        [Fact]
        public void Can_create_file_paths_from_string_with_directory()
        {
            var path = new RelativeFilePath("../readme.md");
            path.Value.Should().Be("../readme.md");    
        }

        [Fact]
        public void Can_create_file_paths_from_string_without_directory()
        {
            var path = new RelativeFilePath("readme.md");
            path.Value.Should().Be("readme.md");
        }

        [Fact]
        public void Normalises_the_passed_path()
        {
            var path = new RelativeFilePath(@"..\readme.md");
            path.Value.Should().Be("../readme.md");
        }

        [Fact]
        public void Throws_exception_if_the_path_contains_invalid_filename_characters()
        {
            Action action = ()=> new RelativeFilePath(@"abc*def");
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Throws_exception_if_the_path_contains_invalid_path_characters()
        {
            Action action = () => new RelativeFilePath(@"abc|def");
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Throws_exception_if_the_path_is_empty()
        {
            Action action = () => new RelativeFilePath("");
            action.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData("../src/Program.cs", "../src")]
        [InlineData("src/Program.cs", "src")]
        [InlineData("Readme.md", "")]
        public void Returns_the_directory_path(string path, string directory)
        {
            var relativePath = new RelativeFilePath(path);
            relativePath.Directory.Value.Should().Be(directory);
        }
    }
}
