using MLS.Agent.Markdown;
using System;
using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using WorkspaceServer;
using WorkspaceServer.Tests;

namespace MLS.Agent.Tests
{
    public class CodeLinkBlockTests
    {
        private readonly CodeLinkBlockParser _parser;
        private readonly InMemoryDirectoryAccessor _directoryAccessor;

        public CodeLinkBlockTests()
        {
            var rootDirectory = Create.EmptyWorkspace(isRebuildablePackage: true).Directory;
            _directoryAccessor = new InMemoryDirectoryAccessor(rootDirectory);
            var registry = PackageRegistry.CreateForTryMode(rootDirectory);
            _parser = new CodeLinkBlockParser(_directoryAccessor, registry);
        }
        [Fact]
        public void It_requires_options_to_initialize()
        {
            var block = new CodeLinkBlock(_parser, 0);
            block.Invoking(b => b.InitializeAsync().Wait())
                .Should().Throw<InvalidOperationException>("Attempted to initialize block before adding options");
        }

        [Fact]
        public void It_requires_initialization_before_getting_text()
        {
            var options = new CodeLinkBlockOptions();
            var block = new CodeLinkBlock(_parser, 0);
            block.AddOptions(options, () => Task.FromResult<IDirectoryAccessor>(null));
            block.Invoking(b => { var x = b.SourceCode; })
                .Should().Throw<InvalidOperationException>($"Attempted to retrieve {nameof(CodeLinkBlock.SourceCode)} from uninitialized {nameof(CodeLinkBlock)}");
        }

        [Fact]
        public async Task It_uses_the_provided_directory_accessor()
        {
            _directoryAccessor.Add(("Program.cs", "foo"));

            var options = new CodeLinkBlockOptions(
                sourceFile: new RelativeFilePath("Program.cs"),
                package: "the-package").WithIsProjectImplicit(true);

            var block = new CodeLinkBlock(_parser, 0);
            block.AddOptions(options, () => Task.FromResult<IDirectoryAccessor>(_directoryAccessor));
            await block.InitializeAsync();
            block.SourceCode.Should().Be("foo");
        }
    }
}
