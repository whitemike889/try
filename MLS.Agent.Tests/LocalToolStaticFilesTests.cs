using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Clockwise;
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

                var toolName = "dotnet-try";

                var dotnet = new Dotnet(dotnetTryDirectory);

                var result = await dotnet.Pack($"/p:Version=2.0.0 {GetAgentCsproj()} -o {dotnetTryDirectory.FullName}");

                result.ThrowOnFailure("Package build failed.");

                var installResult = await InstallDotnetTry(dotnet, toolName, dotnetTryDirectory.FullName, dotnetTryDirectory);

                installResult.ThrowOnFailure("Tool installation failed.");

                var toolExecutable = Path.Combine(dotnetTryDirectory.FullName, toolName);


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

        public Task<CommandLineResult> InstallDotnetTry(Dotnet dotnet,
            string packageName,
            string toolPath,
            DirectoryInfo addSource = null, Budget budget = null)
        {
            var args = $"{packageName} --tool-path {toolPath}";
            if (addSource != null)
            {
                args += $" --add-source \"{addSource}\"";
            }

            return dotnet.Execute("tool install".AppendArgs(args), budget);
        }
    }
}