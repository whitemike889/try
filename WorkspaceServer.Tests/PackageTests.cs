using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using FluentAssertions.Extensions;
using System.Threading.Tasks;
using Clockwise;
using Pocket;
using Xunit;
using Xunit.Abstractions;
using WorkspaceServer.Packaging;
using System.Threading;

namespace WorkspaceServer.Tests
{
    public class PackageTests : IDisposable
    {
        private readonly CompositeDisposable disposables = new CompositeDisposable();

        public PackageTests(ITestOutputHelper output)
        {
            disposables.Add(output.SubscribeToPocketLogger());
            disposables.Add(VirtualClock.Start());
        }

        public void Dispose() => disposables.Dispose();

        [Fact]
        public async Task A_package_is_not_initialized_more_than_once()
        {
            var initializer = new TestPackageInitializer(
                "console",
                "MyProject");

            var package = Create.EmptyWorkspace(initializer: initializer);

            await package.CreateRoslynWorkspaceForRunAsync(new TimeBudget(30.Seconds()));
            await package.CreateRoslynWorkspaceForRunAsync(new TimeBudget(30.Seconds()));

            initializer.InitializeCount.Should().Be(1);
        }

        [Fact]
        public async Task Package_after_create_actions_are_not_run_more_than_once()
        {
            var afterCreateCallCount = 0;

            var initializer = new PackageInitializer(
                "console",
                "test",
                async (_, __) =>
                {
                    await Task.Yield();
                    afterCreateCallCount++;
                });

            var package = Create.EmptyWorkspace(initializer: initializer);

            await package.CreateRoslynWorkspaceForRunAsync(new TimeBudget(30.Seconds()));
            await package.CreateRoslynWorkspaceForRunAsync(new TimeBudget(30.Seconds()));

            afterCreateCallCount.Should().Be(1);
        }

        [Fact]
        public async Task A_package_copy_is_not_reinitialized_if_the_source_was_already_initialized()
        {
            var initializer = new TestPackageInitializer(
                "console",
                "MyProject");

            var original = Create.EmptyWorkspace(initializer: initializer);

            await original.CreateRoslynWorkspaceForLanguageServicesAsync(new TimeBudget(30.Seconds()));

            var copy = await Package.Copy(original);

            await copy.CreateRoslynWorkspaceForLanguageServicesAsync(new TimeBudget(30.Seconds()));

            initializer.InitializeCount.Should().Be(1);
        }

        [Fact]
        public async Task When_package_contains_simple_console_app_then_IsAspNet_is_false()
        {
            var package = await Create.ConsoleWorkspaceCopy();

            await package.CreateRoslynWorkspaceForLanguageServicesAsync(new TimeBudget(30.Seconds()));

            package.IsWebProject.Should().BeFalse();
        }

        [Fact]
        public async Task When_package_contains_aspnet_project_then_IsAspNet_is_true()
        {
            var package = await Create.WebApiWorkspaceCopy();

            await package.CreateRoslynWorkspaceForLanguageServicesAsync(new TimeBudget(30.Seconds()));

            package.IsWebProject.Should().BeTrue();
        }

        [Fact]
        public async Task When_package_contains_simple_console_app_then_entry_point_dll_is_in_the_build_directory()
        {
            var package = Create.EmptyWorkspace(initializer: new PackageInitializer("console", "empty"));

            await package.CreateRoslynWorkspaceForRunAsync(new TimeBudget(30.Seconds()));

            package.EntryPointAssemblyPath.Exists.Should().BeTrue();

            package.EntryPointAssemblyPath
                     .FullName
                     .Should()
                     .Be(Path.Combine(
                             package.Directory.FullName,
                             "bin",
                             "Debug",
                             package.TargetFramework,
                             "empty.dll"));
        }


        [Fact]
        public async Task When_package_contains_aspnet_project_then_entry_point_dll_is_in_the_publish_directory()
        {
            var package = Create.EmptyWorkspace(initializer: new PackageInitializer("webapi", "aspnet.webapi"));

            await package.CreateRoslynWorkspaceForRunAsync(new TimeBudget(30.Seconds()));

            package.EntryPointAssemblyPath.Exists.Should().BeTrue();

            package.EntryPointAssemblyPath
                   .FullName
                   .Should()
                   .Be(Path.Combine(
                           package.Directory.FullName,
                           "bin",
                           "Debug",
                           package.TargetFramework,
                           "publish",
                           "aspnet.webapi.dll"));
        }

        [Fact]
        public async Task If_a_build_is_in_fly_the_second_one_will_wait_and_do_not_continue()
        {
            var buildEvents = new LogEntryList();
            var buildEventsMessages = new List<string>();
            var package = await Create.ConsoleWorkspaceCopy(isRebuildable: true);
            var barrier = new Barrier(2);
            using (LogEvents.Subscribe(e =>
            {
                buildEvents.Add(e);
                buildEventsMessages.Add(e.Evaluate().Message);
                if (e.Evaluate().Message.StartsWith("Attempting building package "))
                {
                    barrier.SignalAndWait(3.Minutes());
                }
            }))
            {
                await Task.WhenAll(
                    Task.Run(() => package.FullBuild()),
                    Task.Run(() => package.FullBuild()));
            }


            buildEventsMessages.Should()
                .Contain(e => e.StartsWith("Building workspace using "))
                .And
                .Contain(e => e.StartsWith("Skipping build for package "));
        }
    }
}
