using System;
using FluentAssertions;
using WorkspaceServer.Packaging;
using Xunit;

namespace WorkspaceServer.Tests
{
    public partial class PackageTests
    {
        [Fact]
        public void It_can_have_assets_added_to_it()
        {
            var directoryAccessor = new InMemoryDirectoryAccessor();

            var package = new Package2(directoryAccessor);

            var projectAsset = new ProjectAsset(directoryAccessor);
            package.Add(projectAsset);

            package.Should().Contain(a => a == projectAsset);
        }

        [Fact]
        public void An_asset_must_be_in_subdirectory_of_the_package()
        {
            var directoryAccessor = new InMemoryDirectoryAccessor();

            var package = new Package2(directoryAccessor.GetDirectoryAccessorForRelativePath("1"));

            var projectAsset = new ProjectAsset(directoryAccessor.GetDirectoryAccessorForRelativePath("2"));

            package.Invoking(p => p.Add(projectAsset)).Should()
                   .Throw<ArgumentException>()
                   .And
                   .Message
                   .Should()
                   .Be("Asset must be located under package path");
        }
    }
}