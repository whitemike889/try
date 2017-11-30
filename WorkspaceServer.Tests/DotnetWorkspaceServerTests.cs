using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using FluentAssertions;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Pocket;
using WorkspaceServer.Servers.Local;
using WorkspaceServer.Servers.OmniSharp;
using Xunit;
using Xunit.Abstractions;

namespace WorkspaceServer.Tests
{
    public class DotnetWorkspaceServerTests : IDisposable
    {
        private static readonly Lazy<DirectoryInfo> projectRoot = new Lazy<DirectoryInfo>(() =>
        {
            var directory = new DirectoryInfo(@"./ProjectTypes/ConsoleApp");

            if (!directory.Exists)
            {
                directory.Create();

                new Dotnet(directory).New("console");
            }

            return directory;
        });

        private readonly CompositeDisposable disposables = new CompositeDisposable();

        public DotnetWorkspaceServerTests(ITestOutputHelper output)
        {
            disposables.Add(LogEvents.Subscribe(e => output.WriteLine(e.ToLogString())));
        }

        public void Dispose() => disposables.Dispose();

        [Fact]
        public async Task OmniSharp_console_output_is_observable()
        {
            using (var omnisharp = new OmniSharpServer(projectRoot.Value))
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

            using (new OmniSharpServer(projectRoot.Value))
            {
            }

            var laterOmnisharpProcessCount = processCount();

            Assert.Equal(omnisharpProcessCount, laterOmnisharpProcessCount);
        }

        [Fact]
        public async Task Omnisharp_loads_the_project_found_in_its_working_directory()
        {
            using (var omnisharp = new OmniSharpServer(projectRoot.Value))
            {
                var output = new ConcurrentQueue<string>();
                using (omnisharp.StandardOutput.Subscribe(s => output.Enqueue(s)))
                {
                    await omnisharp.StandardInput.WriteLineAsync("/project");

                    output.Should().Contain(s => s.Contains("ConsoleApp.csproj"));
                }
            }
        }

        [Fact]
        public void Can_subscribe_to_omisharp_events()
        {
            using (var omnisharp = new OmniSharpServer(projectRoot.Value))
            {
            }

            // TODO (Can_subscribe_to_omisharp_events) write test
            throw new NotImplementedException("Test Can_subscribe_to_omisharp_events is not written yet.");
        }
    }
}
