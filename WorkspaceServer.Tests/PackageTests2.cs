using System;
using System.Linq;
using System.Threading.Tasks;
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

            var package = new Package2("the-package", directoryAccessor);

            var projectAsset = new ProjectAsset(directoryAccessor);
            package.Add(projectAsset);

            package.Assets.Should().Contain(a => a == projectAsset);
        }

        [Fact]
        public void An_asset_must_be_in_subdirectory_of_the_package()
        {
            var directoryAccessor = new InMemoryDirectoryAccessor();

            var package = new Package2("1", directoryAccessor.GetDirectoryAccessorForRelativePath("1"));

            var projectAsset = new ProjectAsset(directoryAccessor.GetDirectoryAccessorForRelativePath("2"));

            package.Invoking(p => p.Add(projectAsset)).Should()
                   .Throw<ArgumentException>()
                   .And
                   .Message
                   .Should()
                   .StartWith("Asset must be located under package path");
        }

        [Fact]
        public async Task A_package_discovers_project_assets_in_its_root()
        {
            var package = new Package2(
                "the-package",
                new InMemoryDirectoryAccessor
                {
                    ("myapp.csproj", "")
                });

            await package.EnsureLoadedAsync();

            package.Assets.Should().ContainSingle(a => a is ProjectAsset);
            package.Assets.Single().DirectoryAccessor.FileExists("myapp.csproj").Should().BeTrue();
        }

        [Fact]
        public async Task A_package_discovers_project_assets_in_subfolders()
        {
            var package = new Package2(
                "the-package",
                new InMemoryDirectoryAccessor
                {
                    ("./subfolder/myapp.csproj", "")
                });

            await package.EnsureLoadedAsync();

            package.Assets.Should().ContainSingle(a => a is ProjectAsset);
            package.Assets.Single().DirectoryAccessor.FileExists("myapp.csproj").Should().BeTrue();
        }
    }
}