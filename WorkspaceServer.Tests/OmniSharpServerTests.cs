using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using FluentAssertions;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using MLS.Agent.Tools;
using Pocket;
using WorkspaceServer.Servers.OmniSharp;
using Xunit;
using Xunit.Abstractions;

namespace WorkspaceServer.Tests
{
    public class OmniSharpServerTests : IDisposable
    {
        private static readonly Workspace workspace = new Workspace(nameof(OmniSharpServerTests));

        private readonly CompositeDisposable disposables = new CompositeDisposable();

        public OmniSharpServerTests(ITestOutputHelper output)
        {
            Task.Run(() => workspace.EnsureCreated()).Wait();
            disposables.Add(output.SubscribeToPocketLogger());
        }

        public void Dispose() => disposables.Dispose();

        [Fact]
        public async Task OmniSharp_console_output_is_observable()
        {
            using (var omniSharp = new OmniSharpServer(workspace.Directory))
            {
                var observer = new Subject<string>();

                using (omniSharp.StandardOutput.Subscribe(observer))
                {
                    await omniSharp.WorkspaceReady();
                    await observer.FirstOrDefaultAsync().Timeout(5.Seconds());
                }
            }
        }

        [Fact]
        public void OmniSharp_process_is_killed_when_omnisharp_is_disposed()
        {
            int processCount() => Process.GetProcesses().Count(p => p.ProcessName.IndexOf("omnisharp", StringComparison.OrdinalIgnoreCase) > 0);

            var omnisharpProcessCount = processCount();

            using (new OmniSharpServer(workspace.Directory))
            {
            }

            var laterOmnisharpProcessCount = processCount();

            laterOmnisharpProcessCount.Should().Be(omnisharpProcessCount);
        }

        [Fact]
        public async Task Omnisharp_loads_the_project_found_in_its_working_directory()
        {
            using (var omniSharp = new OmniSharpServer(workspace.Directory))
            {
                var output = new ConcurrentQueue<string>();

                using (omniSharp.StandardOutput.Subscribe(s => output.Enqueue(s)))
                {
                    await omniSharp.WorkspaceReady();
                    var projectName = $"{nameof(OmniSharpServerTests)}.csproj";

                    await omniSharp
                          .StandardOutput
                          .FirstAsync(e => e.Contains(projectName))
                          .Timeout(5.Seconds());

                    output.Should().Contain(s => s.Contains(projectName));
                }
            }
        }

        [Fact]
        public async Task CodeCheck_can_be_read_compilation_errors_after_a_buffer_update()
        {
            using (var omniSharp = StartOmniSharp(workspace.Directory))
            {
                await omniSharp.WorkspaceReady(Default.Timeout());

                var file = await omniSharp.FindFile("Program.cs", Default.Timeout());

                var code = await file.ReadAsync();

                await omniSharp.UpdateBuffer(
                    file,
                    code.Replace(";", ""),
                    Default.Timeout());

                var diagnostics = await omniSharp.CodeCheck(timeout: Default.Timeout());

                diagnostics.Body
                           .QuickFixes
                           .Should()
                           .Contain(d => d.Text.Contains("; expected"));
            }
        }

        [Fact]
        public async Task Workspace_information_can_be_requested()
        {
            using (var omniSharp = new OmniSharpServer(workspace.Directory, logToPocketLogger: true))
            {
                await omniSharp.WorkspaceReady();

                var response = await omniSharp.GetWorkspaceInformation();

                response.Success
                        .Should()
                        .BeTrue();

                response.Body
                        .MSBuildSolution
                        .Projects
                        .Should()
                        .HaveCount(1);

                response.Body
                        .MSBuildSolution
                        .Projects
                        .Single()
                        .SourceFiles
                        .Should()
                        .Contain(f => f.Name == "Program.cs");
            }
        }

        private OmniSharpServer StartOmniSharp(DirectoryInfo projectDirectory = null) =>
            new OmniSharpServer(
                projectDirectory ?? workspace.Directory,
                logToPocketLogger: true);
    }
}
