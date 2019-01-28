using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FluentAssertions;
using MLS.Agent.Tools;
using Recipes;
using WorkspaceServer;
using WorkspaceServer.PackageDiscovery;
using Xunit;

namespace MLS.Agent.Tests
{
    public class LocalToolStaticFilesTests
    {
        [Fact]
        public async Task Can_load_css_file()
        {
            var disposableDirectory = DisposableDirectory.Create();
            
                var tempDir = disposableDirectory.Directory;
                
                var dotnet = new Dotnet(tempDir);
               
                var result = await dotnet.Pack($"{GetAgentCsproj()} -o {tempDir.FullName}");

                result.ThrowOnFailure("Package build failed.");

                var installResult = await dotnet.ToolInstall("dotnet-try", tempDir.FullName, tempDir);

                installResult.ThrowOnFailure("Tool installation failed.");

                var toolExecutable = Path.Combine(tempDir.FullName, "dotnet-try.exe");

                var proc = Process.StartProcess(toolExecutable, "", tempDir.FullName);

                var client = new HttpClient();
                await Task.Delay(3000);
                var response = await client.GetAsync(new Uri(@"http://localhost:5000/css/trydotnet.css"));

                response.EnsureSuccess();

                proc.Kill();
                proc.WaitForExit();
                disposableDirectory.Dispose();
        }

        private static string GetAgentCsproj([CallerFilePath] string path = null)
        {
            return Path.Combine(Path.GetDirectoryName(path), @"..", "MLS.Agent", "MLS.Agent.csproj");
        }
    }
}