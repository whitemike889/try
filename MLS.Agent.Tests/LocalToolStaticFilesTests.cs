using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
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
        public async Task dotnet_try_tools_packs_static_file_content()
        {
            var dotnetTryDirectory = Package.CreateDirectory("dotnet-try-test");

            var toolName = "dotnet-try";

            var dotnet = new Dotnet(dotnetTryDirectory);

            var packResult = await dotnet.Pack($"/p:Version=2.0.0 {GetAgentCsproj()} -o {dotnetTryDirectory.FullName}");

            packResult.ThrowOnFailure("Package build failed.");

            var installResult = await InstallDotnetTry(dotnet, toolName, dotnetTryDirectory.FullName, dotnetTryDirectory);

            installResult.ThrowOnFailure("Tool installation failed.");

            var agentDll = Directory.GetFiles(dotnetTryDirectory.FullName, "MLS.Agent.dll", SearchOption.AllDirectories).Single();

            var agentDirectory = new DirectoryInfo(Path.GetDirectoryName(agentDll));

            var staticContentDirectory = agentDirectory.GetDirectories("wwwroot").Single();
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