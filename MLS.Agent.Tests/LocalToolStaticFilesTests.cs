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
using WorkspaceServer.Packaging;
using Xunit;

namespace MLS.Agent.Tests
{
    public class LocalToolStaticFilesTests
    {
        [Fact]
        public async Task Can_load_css_file()
        {
            System.Diagnostics.Process toolProcess = null;
            try
            {
                var dotnetTryDirectory = Package.CreateDirectory("dotnet-try-test");

                var dotnet = new Dotnet(dotnetTryDirectory);

                var result = await dotnet.Pack($"{GetAgentCsproj()} -o {dotnetTryDirectory.FullName}");

                result.ThrowOnFailure("Package build failed.");

                var installResult = await dotnet.ToolInstall("dotnet-try", dotnetTryDirectory.FullName, dotnetTryDirectory);

                installResult.ThrowOnFailure("Tool installation failed.");

                var toolExecutable = Path.Combine(dotnetTryDirectory.FullName, "dotnet-try");

                
                toolProcess = Process.StartProcess(toolExecutable, " .", dotnetTryDirectory.FullName);

                var client = new HttpClient();

                var response = await client.GetAsync(new Uri(@"http://localhost:5000/css/trydotnet.css"));

                response.EnsureSuccess();
            }
            finally
            {
                toolProcess?.Kill();
                toolProcess?.WaitForExit(2000);
            }
        }

        private static string GetAgentCsproj([CallerFilePath] string path = null)
        {
            return Path.Combine(Path.GetDirectoryName(path), @"..", "MLS.Agent", "MLS.Agent.csproj");
        }
    }
}