using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MLS.Agent.Tools;
using Pocket;
using Xunit;
using Xunit.Abstractions;

namespace WorkspaceServer.Tests
{
    public class WorkspaceTests: IDisposable
    {
        private readonly CompositeDisposable disposables = new CompositeDisposable();

        public WorkspaceTests(ITestOutputHelper output)
        {
            disposables.Add(output.SubscribeToPocketLogger());
        }

        public void Dispose() => disposables.Dispose();
    
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

        [Fact]
        public async Task EnsureBuilt_is_safe_for_concurrency()
        {
            var workspace = new Workspace($"{nameof(EnsureBuilt_is_safe_for_concurrency)}.{DateTime.Now.ToString("MM-dd-hh-ss")}");

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
            var workspace = new Workspace($"{nameof(EnsureCreated_is_safe_for_concurrency)}.{DateTime.Now.ToString("MM-dd-hh-ss")}");

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
    }
}