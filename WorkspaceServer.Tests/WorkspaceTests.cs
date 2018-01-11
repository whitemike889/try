using System;
using System.Threading.Tasks;
using FluentAssertions;
using MLS.Agent.Tools;
using Xunit;

namespace WorkspaceServer.Tests
{
    public class WorkspaceTests
    {
        [Fact]
        public async Task A_workspace_is_not_initialized_more_than_once()
        {
            var initializer = new InMemoryWorkspaceInitializer();

            var workspace = new Workspace(
                $"{nameof(A_workspace_is_not_initialized_more_than_once)}.{Guid.NewGuid()}",
                initializer: initializer);

            await workspace.EnsureCreated();
            await workspace.EnsureCreated();

            initializer.InitializeCount.Should().Be(1);
        }

        [Fact]
        public async Task A_workspace_copy_is_not_reinitialized_if_the_source_was_already_built()
        {
            var initializer = new InMemoryWorkspaceInitializer();

            var original = new Workspace(
                $"{nameof(A_workspace_copy_is_not_reinitialized_if_the_source_was_already_built)}.{Guid.NewGuid()}",
                initializer: initializer);

            await original.EnsureCreated();

            var copy = Workspace.Copy(original);

            await copy.EnsureCreated();

            initializer.InitializeCount.Should().Be(1);
        }
    }
}