using System;
using System.IO;
using FluentAssertions;
using FluentAssertions.Extensions;
using System.Threading;
using System.Threading.Tasks;
using Clockwise;
using Pocket;
using Xunit;
using Xunit.Abstractions;
using WorkspaceServer.Workspaces;

namespace WorkspaceServer.Tests
{
    public class WorkspaceBuildTests : IDisposable
    {
        private readonly CompositeDisposable disposables = new CompositeDisposable();

        public WorkspaceBuildTests(ITestOutputHelper output)
        {
            disposables.Add(output.SubscribeToPocketLogger());
            disposables.Add(VirtualClock.Start());
        }

        public void Dispose() => disposables.Dispose();

        [Fact]
        public async Task A_workspace_is_not_initialized_more_than_once()
        {
            var initializer = new FakeWorkspaceInitializer();

            var workspace = Create.EmptyWorkspace(initializer: initializer);

            await workspace.EnsureCreated();
            await workspace.EnsureCreated();

            initializer.InitializeCount.Should().Be(1);
        }

        [Fact]
        public async Task Workspace_after_create_actions_are_not_run_more_than_once()
        {
            var afterCreateCallCount = 0;

            var initializer = new WorkspaceInitializer(
                "console",
                "test",
                async (_, __) => afterCreateCallCount++);

            var workspace = Create.EmptyWorkspace(initializer: initializer);

            await workspace.EnsureCreated();
            await workspace.EnsureCreated();

            afterCreateCallCount.Should().Be(1);
        }

        [Fact]
        public async Task A_workspace_copy_is_not_reinitialized_if_the_source_was_already_built()
        {
            var initializer = new FakeWorkspaceInitializer();

            var original = Create.EmptyWorkspace(initializer: initializer);

            await original.EnsureCreated();

            var copy = WorkspaceBuild.Copy(original);

            await copy.EnsureCreated();

            initializer.InitializeCount.Should().Be(1);
        }

        [Fact]
        public async Task EnsureBuilt_is_safe_for_concurrency()
        {
            var workspace = Create.EmptyWorkspace();

            var barrier = new Barrier(2);

            async Task EnsureBuilt()
            {
                await Task.Yield();
                barrier.SignalAndWait(20.Seconds());
                await workspace.EnsureBuilt();
            }

            await Task.WhenAll(
                EnsureBuilt(),
                EnsureBuilt());
        }

        [Fact]
        public async Task EnsureCreated_is_safe_for_concurrency()
        {
            var workspace = Create.EmptyWorkspace();

            var barrier = new Barrier(2);

            async Task EnsureCreated()
            {
                await Task.Yield();
                barrier.SignalAndWait(20.Seconds());
                await workspace.EnsureCreated();
            }

            await Task.WhenAll(
                EnsureCreated(),
                EnsureCreated());
        }

        [Fact]
        public async Task EnsurePublished_is_safe_for_concurrency()
        {
            var workspace = Create.EmptyWorkspace();

            var barrier = new Barrier(2);

            async Task EnsurePublished()
            {
                await Task.Yield();
                barrier.SignalAndWait(20.Seconds());
                await workspace.EnsurePublished();
            }

            await Task.WhenAll(
                EnsurePublished(),
                EnsurePublished());
        }

        [Fact]
        public async Task When_workspace_contains_simple_console_app_then_IsAspNet_is_false()
        {
            var workspace = await Create.ConsoleWorkspaceCopy();

            await workspace.EnsureCreated();

            workspace.IsWebProject.Should().BeFalse();
        }

        [Fact]
        public async Task When_workspace_contains_aspnet_project_then_IsAspNet_is_true()
        {
            var workspace = await Create.WebApiWorkspaceCopy();

            await workspace.EnsureCreated();

            workspace.IsWebProject.Should().BeTrue();
        }

        [Fact]
        public async Task When_workspace_contains_simple_console_app_then_entry_point_dll_is_in_the_build_directory()
        {
            var workspace = await Create.ConsoleWorkspaceCopy();

            await workspace.EnsurePublished();

            workspace.EntryPointAssemblyPath.Exists.Should().BeTrue();

            workspace.EntryPointAssemblyPath
                     .FullName
                     .Should()
                     .Be(Path.Combine(
                             workspace.Directory.FullName,
                             "bin",
                             "Debug",
                             workspace.TargetFramework,
                             "console.dll"));
        }

        [Fact]
        public async Task When_workspace_contains_aspnet_project_then_entry_point_dll_is_in_the_publish_directory()
        {
            var workspace = await Create.WebApiWorkspaceCopy();

            await workspace.EnsurePublished();

            workspace.EntryPointAssemblyPath.Exists.Should().BeTrue();

            workspace.EntryPointAssemblyPath
                     .FullName
                     .Should()
                     .Be(Path.Combine(
                             workspace.Directory.FullName,
                             "bin",
                             "Debug",
                             workspace.TargetFramework,
                             "publish",
                             "aspnet.webapi.dll"));
        }
    }
}
