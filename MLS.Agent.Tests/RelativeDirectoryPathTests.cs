using FluentAssertions;
using System;
using Xunit;

namespace MLS.Agent.Tests
{
    public class RelativeDirectoryPathTests
    {
        [Fact]
        public void Can_create_directory_paths_from_string_with_directory()
        {
            var path = new RelativeDirectoryPath("../src");
            path.Value.Should().Be("../src");
        }

        [Fact]
        public void Normalises_the_passed_path()
        {
            var path = new RelativeDirectoryPath(@"..\src");
            path.Value.Should().Be("../src");
        }

        [Fact]
        public void Throws_exception_if_the_path_contains_invalid_filename_characters()
        {
            Action action = () => new RelativeDirectoryPath(@"abc*def");
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Throws_exception_if_the_path_contains_invalid_path_characters()
        {
            Action action = () => new RelativeDirectoryPath(@"abc|def");
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Returns_an_empty_path_if_an_empty_path_is_passed()
        {
            var path =  new RelativeDirectoryPath("");
            path.Value.Should().Be("");
        }
    }
}
