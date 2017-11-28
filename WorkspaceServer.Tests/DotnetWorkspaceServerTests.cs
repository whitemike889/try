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

namespace WorkspaceServer.Tests
{
    public class DotnetWorkspaceServerTests : IDisposable
    {
        private readonly CompositeDisposable disposables = new CompositeDisposable();

        public DotnetWorkspaceServerTests(ITestOutputHelper output)
        {
            disposables.Add(LogEvents.Subscribe(e => output.WriteLine(e.ToLogString())));
        }

        public void Dispose() => disposables.Dispose();



        [Fact]
        public async Task OmniSharp_console_output_is_observable()
        {
            using (var omnisharp = new OmniSharp(@"c:\temp\MyConsoleApp"))
            {
                var observer = new Subject<string>();

                using (omnisharp.StandardOut.Subscribe(observer))
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

            using (new OmniSharp(@"c:\temp\MyConsoleApp"))
            {
            }

            var laterOmnisharpProcessCount = processCount();

            Assert.Equal(omnisharpProcessCount, laterOmnisharpProcessCount);
        }

        [Fact]
        public async Task Omnisharp_responds_to_project_request()
        {
            using (var omnisharp = new OmniSharp(@"c:\temp\MyConsoleApp"))
            {
                var output = new ConcurrentQueue<string>();
                using (omnisharp.StandardOut.Subscribe(s => output.Enqueue(s)))
                {
                    await omnisharp.StandardInput.WriteLineAsync("/project");

                    output.Should().Contain(s => s.Contains("MyConsoleApp.csproj"));
                }
            }
        }
    }
}
