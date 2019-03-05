using System;
using FluentAssertions;
using System.Threading.Tasks;
using Clockwise;
using Pocket;
using Xunit;
using Xunit.Abstractions;
using WorkspaceServer.Packaging;
using System.IO;
using FluentAssertions.Extensions;
using System.Linq;
using Microsoft.Reactive.Testing;

namespace WorkspaceServer.Tests
{
    public class RebuildablePackageTests : IDisposable
    {
        private readonly CompositeDisposable disposables = new CompositeDisposable();

        public RebuildablePackageTests(ITestOutputHelper output)
        {
            disposables.Add(output.SubscribeToPocketLogger());
            disposables.Add(VirtualClock.Start());
        }

        public void Dispose() => disposables.Dispose();

        [Fact]
        public async Task If_a_new_file_is_added_and_ensure_ready_is_called_the_analyzer_result_includes_the_file()
        {
            var package = (RebuildablePackage) await Create.ConsoleWorkspaceCopy(isRebuildable: true);
            await package.EnsureReady(new TimeBudget(30.Seconds()));

            var newFile = Path.Combine(package.Directory.FullName, "Sample.cs");
            package.AnalyzerResult.SourceFiles.Should().NotContain(newFile);

            File.WriteAllText(newFile, "//this is a new file");

            await package.EnsureReady(new TimeBudget(30.Seconds()));

            package.AnalyzerResult.SourceFiles.Should().Contain(newFile);
        }

        [Fact]
        public async Task If_the_project_file_is_changed_and_ensure_ready_is_called_analyzer_result_reflects_the_changes()
        {
            var package = (RebuildablePackage)await Create.ConsoleWorkspaceCopy(isRebuildable: true);
            await package.EnsureReady(new TimeBudget(30.Seconds()));

            package.AnalyzerResult.PackageReferences.Keys.Should().NotContain("Microsoft.CodeAnalysis");

            await new Dotnet(package.Directory).AddPackage("Microsoft.CodeAnalysis", "2.8.2");

            await package.EnsureReady(new TimeBudget(30.Seconds()));
            package.AnalyzerResult.PackageReferences.TryGetValue("Microsoft.CodeAnalysis", out var reference);
            reference.Should().NotBeNull();
            reference["Version"].Should().Be("2.8.2");
        }

        [Fact]
        public async Task If_an_existing_file_is_deleted_and_ensure_ready_is_called_then_the_analyzer_result_doesnt_include_the_file()
        {
            var package = (RebuildablePackage)await Create.ConsoleWorkspaceCopy(isRebuildable: true);
            await package.EnsureReady(new TimeBudget(30.Seconds()));

            var existingFile = Path.Combine(package.Directory.FullName, "Program.cs");
            package.AnalyzerResult.SourceFiles.Should().Contain(existingFile);

            File.Delete(existingFile);

            await package.EnsureReady(new TimeBudget(30.Seconds()));

            package.AnalyzerResult.SourceFiles.Should().NotContain(existingFile);
        }

        [Fact]
        public async Task If_an_existing_file_is_modified_and_ensure_ready_is_called_then_the_analyzer_result_is_updated()
        {
            var package = (RebuildablePackage)await Create.ConsoleWorkspaceCopy(isRebuildable: true);
            await package.EnsureReady(new TimeBudget(30.Seconds()));

            var existingFile = Path.Combine(package.Directory.FullName, "Program.cs");
            var oldAnalyzerResult = package.AnalyzerResult;
            File.WriteAllText(existingFile, "//this is Program.cs");

            await package.EnsureReady(new TimeBudget(30.Seconds()));

            package.AnalyzerResult.Should().NotBeSameAs(oldAnalyzerResult);
        }

        [Fact]
        public async Task If_a_build_is_in_progress_and_another_request_comes_in_both_are_resolved_using_the_final_one()
        {
            var vt = new TestScheduler();
            var package = (RebuildablePackage)await Create.ConsoleWorkspaceCopy(isRebuildable: true, buildThrottleScheduler:vt);
            var workspace1 = package.CreateRoslynWorkspaceAsync(new TimeBudget(30.Seconds()));
            vt.AdvanceBy(TimeSpan.FromSeconds(0.2).Ticks);
            var newFile = Path.Combine(package.Directory.FullName, "Sample.cs");
            File.WriteAllText(newFile, "//this is Sample.cs");
            vt.AdvanceBy(TimeSpan.FromSeconds(0.2).Ticks);
            var workspace2 = package.CreateRoslynWorkspaceAsync(new TimeBudget(30.Seconds()));
            vt.AdvanceBy(TimeSpan.FromSeconds(0.6).Ticks);


            workspace1.Should().BeSameAs(workspace2);

            var workspaces = await Task.WhenAll(workspace1, workspace2);
            
            workspaces[0].CurrentSolution.Projects.First().Documents.Should().Contain(p => p.FilePath.EndsWith("Sample.cs"));
            workspaces[1].CurrentSolution.Projects.First().Documents.Should().Contain(p => p.FilePath.EndsWith("Sample.cs"));
        }
    }
}
