using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using MLS.Agent.Tools;
using OmniSharp.Client.Commands;
using Pocket;
using WorkspaceServer.Servers.Dotnet;
using Xunit;
using Xunit.Abstractions;

namespace WorkspaceServer.Tests
{
    public class EmitTests : IDisposable
    {
        private readonly CompositeDisposable disposables = new CompositeDisposable();

        public EmitTests(ITestOutputHelper output)
        {
            disposables.Add(output.SubscribeToPocketLogger());
        }

        public void Dispose() => disposables.Dispose();

        [Fact]
        public async Task Console_app_project_can_be_emitted()
        {
            using (var omniSharp = await StartOmniSharp())
            {
                await omniSharp.WorkspaceReady();

                var response = await omniSharp.SendCommand<Emit, EmitResponse>(new Emit());

                File.Exists(response.Body.OutputAssemblyPath).Should().BeTrue();
            }
        }

        [Fact]
        public async Task Emitted_console_app_project_can_be_updated_and_rerun()
        {
            using (var omniSharp = await StartOmniSharp())
            {
                await omniSharp.WorkspaceReady();

                var file = await omniSharp.FindFile("Program.cs");

                var code = await file.ReadAsync();

                await omniSharp.UpdateBuffer(
                    file,
                    code.Replace("Hello World", "Hola mundo"));

                var (output, error) = await EmitAndRun(omniSharp);

                output.Should().Contain("Hola mundo!");
            }
        }

        private async Task<OmniSharpServer> StartOmniSharp() =>
            new OmniSharpServer(
                (await Create.TestWorkspace(nameof(EmitTests))).Directory,
                Paths.EmitPlugin,
                logToPocketLogger: true);

        private async Task<(IReadOnlyCollection<string>, IReadOnlyCollection<string>)> ExecuteEmittedAssembly(string dllPath)
        {
            var result = await new Dotnet().Execute(dllPath);

            return (result.Output, result.Error);
        }

        private async Task<(IReadOnlyCollection<string> output, IReadOnlyCollection<string> error)> EmitAndRun(
            OmniSharpServer omnisharp)
        {
            var response = await omnisharp.Emit();

            return await ExecuteEmittedAssembly(response.Body.OutputAssemblyPath);
        }
    }
}
