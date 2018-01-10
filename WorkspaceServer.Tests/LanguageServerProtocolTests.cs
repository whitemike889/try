using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MLS.Agent.Tools;
using OmniSharp.Extensions.LanguageServer.Client;
using OmniSharp.Extensions.LanguageServer.Client.Processes;
using Pocket;
using Pocket.For.MicrosoftExtensionsLogging;
using Xunit;
using Xunit.Abstractions;
using static Pocket.Logger;

namespace WorkspaceServer.Tests
{
    public class LanguageServerProtocolTests : IDisposable
    {
        private readonly ILoggerFactory loggerFactory = new LoggerFactory()
            .AddPocketLogger();

        private static readonly Lazy<Workspace> workspace = new Lazy<Workspace>(() =>
        {
            var workspace = new Workspace(nameof(LanguageServerProtocolTests));
            workspace.EnsureCreated("console");
            return workspace;
        });

        private readonly CompositeDisposable disposables = new CompositeDisposable();

        public LanguageServerProtocolTests(ITestOutputHelper output)
        {
            disposables.Add(LogEvents.Subscribe(e => output.WriteLine(e.ToLogString())));
        }

        public void Dispose() => disposables.Dispose();

        [Fact(Skip = "Not today")]
        public async Task Hello_LSP_completions()
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = @"C:\dev\github\omnisharp-roslyn\artifacts\publish\OmniSharp.Stdio\win7-x64\omnisharp.exe",
                Arguments = "-lsp"
            };

            using (var serverProcess = new StdioServerProcess(loggerFactory, processStartInfo))
            using (var client = new LanguageClient(
                loggerFactory,
                serverProcess))
            {
                await client.Initialize(workspace.Value.Directory.FullName);

                var completions = await client.TextDocument.Completions(
                                      @"C:\Users\josequ\.trydotnet\projects\LanguageServerProtocolTests\Program.cs", 9, 21);

                Log.Trace("Completions: {c}", completions);
            }

            // FIX (Completions) write test
            throw new NotImplementedException();
        }
    }
}
