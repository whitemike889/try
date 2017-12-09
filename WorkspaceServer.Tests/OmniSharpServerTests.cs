using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reactive.Linq;
using FluentAssertions;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Pocket;
using WorkspaceServer.Servers.OmniSharp;
using Xunit;
using Xunit.Abstractions;
using static Pocket.Logger<WorkspaceServer.Tests.OmniSharpServerTests>;

namespace WorkspaceServer.Tests
{
    public class OmniSharpServerTests : IDisposable
    {
        private static readonly Lazy<Project> project = new Lazy<Project>(() =>
        {
            var project = new Project(nameof(OmniSharpServerTests));
            project.IfEmptyInitializeFromDotnetTemplate("console");
            return project;
        });

        private readonly CompositeDisposable disposables = new CompositeDisposable();

        public OmniSharpServerTests(ITestOutputHelper output)
        {
            disposables.Add(LogEvents.Subscribe(e => output.WriteLine(e.ToLogString())));
        }

        public void Dispose() => disposables.Dispose();

        [Fact]
        public async Task OmniSharp_console_output_is_observable()
        {
            using (var omnisharp = new OmniSharpServer(project.Value.Directory))
            {
                var observer = new Subject<string>();

                using (omnisharp.StandardOutput.Subscribe(observer))
                {
                    await observer.FirstOrDefaultAsync().Timeout(5.Seconds());
                }
            }
        }

        [Fact]
        public void OmniSharp_process_is_killed_when_omnisharp_is_disposed()
        {
            int processCount() => Process.GetProcesses().Count(p => p.ProcessName.IndexOf("omnisharp", StringComparison.OrdinalIgnoreCase) > 0);

            var omnisharpProcessCount = processCount();

            using (new OmniSharpServer(project.Value.Directory))
            {
            }

            var laterOmnisharpProcessCount = processCount();

            laterOmnisharpProcessCount.Should().Be(omnisharpProcessCount);
        }

        [Fact]
        public async Task Omnisharp_loads_the_project_found_in_its_working_directory()
        {
            using (var omnisharp = new OmniSharpServer(project.Value.Directory))
            {
                var output = new ConcurrentQueue<string>();

                using (omnisharp.StandardOutput.Subscribe(s => output.Enqueue(s)))
                {
                    var projectName = $"{nameof(OmniSharpServerTests)}.csproj";

                    await omnisharp
                        .StandardOutput
                        .FirstAsync(e => e.Contains(projectName))
                        .Timeout(5.Seconds());

                    output.Should().Contain(s => s.Contains(projectName));
                }
            }
        }

        [Fact(Skip = "not yet")]
        public void Can_subscribe_to_omisharp_events()
        {
            using (var omnisharp = new OmniSharpServer(project.Value.Directory))
            {
            }

            // TODO (Can_subscribe_to_omisharp_events) write test
            throw new NotImplementedException("Test Can_subscribe_to_omisharp_events is not written yet.");
        }
    }
}
