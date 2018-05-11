using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions.Extensions;
using MLS.Agent.Tools;
using Pocket;
using WorkspaceServer.Servers.Dotnet;
using Xunit;
using Xunit.Abstractions;
using System.Reactive.Subjects;
using System.Collections.Generic;

namespace WorkspaceServer.Tests
{
    public class OmniSharpServerTests : IDisposable
    {
        private static readonly Workspace Workspace = new Workspace(nameof(OmniSharpServerTests));

        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public OmniSharpServerTests(ITestOutputHelper output)
        {
            Task.Run(() => Workspace.EnsureCreated()).Wait();
            _disposables.Add(output.SubscribeToPocketLogger());
        }

        public void Dispose() => _disposables.Dispose();

        [Fact]
        public async Task OmniSharp_console_output_is_observable()
        {
            using (var omniSharp = new OmniSharpServer(Workspace.Directory, logToPocketLogger: true))
            {
                await omniSharp.WorkspaceReady();
                await omniSharp.StandardOutput.FirstOrDefaultAsync().Timeout(10.Seconds());
                await AssertVersionLoaded("v1.29.0-beta1", omniSharp);
            }
        }

        [Fact]
        public async Task Can_load_version_from_dottrydotnet_file()
        {
            var info = new DirectoryInfo(Path.Combine(Paths.UserProfile,
                             ".trydotnet",
                             "ca29"));

            var workspace = new Workspace(info, "ca29");
            await workspace.EnsureCreated();
            File.WriteAllText(Path.Combine(info.FullName, ".trydotnet"), "ca-2.9.2");

            using (var omniSharp = new OmniSharpServer(workspace.Directory, logToPocketLogger: true))
            {
                await omniSharp.WorkspaceReady();
                await omniSharp.StandardOutput.FirstOrDefaultAsync().Timeout(10.Seconds());
                await AssertVersionLoaded("ca-2.9.2", omniSharp);
            }
        }

        private async Task AssertVersionLoaded(string version, OmniSharpServer omnisharp)
        {
            // OmniSharp will have printed a message saying it located its standalone MSBuild
            // Unfortunately, it doesn't print it's own path on starup.
            Assert.NotNull(await omnisharp.StandardOutput.FirstOrDefaultAsync(o => o.Contains(version)).Timeout(30.Seconds()));
        }

        [Fact]
        public void OmniSharp_process_is_killed_when_omnisharp_is_disposed()
        {
            int processCount() => Process.GetProcesses().Count(p => p.ProcessName.IndexOf("omnisharp", StringComparison.OrdinalIgnoreCase) > 0);

            var omnisharpProcessCount = processCount();

            using (new OmniSharpServer(Workspace.Directory))
            {
            }

            var laterOmnisharpProcessCount = processCount();

            laterOmnisharpProcessCount.Should().Be(omnisharpProcessCount);
        }

        [Fact]
        public async Task Omnisharp_loads_the_project_found_in_its_working_directory()
        {
            using (var omniSharp = new OmniSharpServer(Workspace.Directory))
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
            using (var omniSharp = StartOmniSharp(Workspace.Directory))
            {
                await omniSharp.WorkspaceReady();

                var file = await omniSharp.FindFile("Program.cs");

                var code = await file.ReadAsync();

                await omniSharp.UpdateBuffer(
                    file,
                    code.Replace(";", ""));

                var diagnostics = await omniSharp.CodeCheck();

                diagnostics.Body
                           .QuickFixes
                           .Should()
                           .Contain(d => d.Text.Contains("; expected"));
            }
        }

        [Fact]
        public async Task Workspace_information_can_be_requested()
        {
            using (var omniSharp = new OmniSharpServer(Workspace.Directory, logToPocketLogger: true))
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
                projectDirectory ?? Workspace.Directory,
                logToPocketLogger: true);
    }
}
