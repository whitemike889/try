using System;
using System.IO;
using WorkspaceServer.Packaging;
using Xunit;
using MLS.Agent.Tools;
using FluentAssertions;

namespace WorkspaceServer.Tests
{
    public class PackageAssetTests
    {
        [Fact]
        public void It_has_a_directory()
        {
            DirectoryInfo subDir = Package.DefaultPackagesDirectory.Subdirectory("console").NormalizeEnding();
            var directoryAccessor = new InMemoryDirectoryAccessor(subDir);
            var packageAsset = new PackageAsset(directoryAccessor);
            packageAsset.Directory.FullName.Should().Be(subDir.FullName);
        }

        [Fact]
        public void It_has_a_set_of_files()
        {
            DirectoryInfo subDir = Package.DefaultPackagesDirectory.Subdirectory("console").NormalizeEnding();
            var directoryAccessor = new InMemoryDirectoryAccessor(subDir)
            {
                ("abc.txt", "sample_file")
            };

            var packageAsset = new PackageAsset(directoryAccessor);
            var expectedFile = new FileInfo(Path.Combine(subDir.FullName,"abc.txt")) ;
            packageAsset.GetFiles().Should().ContainSingle(f => f.FullName == expectedFile.FullName);
        }

        [Fact]
        public void It_has_an_associated_package()
        {
            throw new NotImplementedException();
        }
    }

    public partial class PackageWithAssetTests
    {
        [Fact]
        public void It_has_some_assets()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void It_has_a_directory()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void It_has_a_name()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void All_assets_exist_in_the_directory()
        {
            throw new NotImplementedException();
        }
    }

    /* ProjectAsset: PackageAsset
     *      * You can build it
     *      * You can design time build it
     *      * You can get a workspace from it
     *      * Has a way to understand that the package can be used(has all the necessary files in place)
     *     
     * BlazorAsset: PackageAsset (this refers to the static content that will be served)
     * Package 
     *          * will have asset
     */
}
