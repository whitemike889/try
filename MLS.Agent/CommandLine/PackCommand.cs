using System;
using System.CommandLine;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using MLS.Agent.Tools;
using WorkspaceServer;

namespace MLS.Agent.CommandLine
{
    public static class PackCommand
    {
        public static async Task Do(PackOptions options, IConsole console)
        {
            console.Out.WriteLine($"Creating package-tool from {options.PackTarget.FullName}");

            using (var disposableDirectory = DisposableDirectory.Create())
            {
                var tempDir = disposableDirectory.Directory;
                var archivePath = Path.Combine(tempDir.FullName, "packagey.zip");

                ZipFile.CreateFromDirectory(options.PackTarget.FullName, archivePath);
                console.Out.WriteLine(archivePath);

                var files = options.PackTarget.GetFiles();
                var csproj = files.Single(f => f.Extension.Contains("csproj"));
                var name = Path.GetFileNameWithoutExtension(csproj.Name);

                var projectFilePath = Path.Combine(tempDir.FullName, "package-tool.csproj");
                var contentFilePath = Path.Combine(tempDir.FullName, "program.cs");

                await File.WriteAllTextAsync(
                    projectFilePath,
                    Resources.ReadManifestResource("MLS.Agent.MLS.PackageTool.csproj"));

                await File.WriteAllTextAsync(contentFilePath, Resources.ReadManifestResource("MLS.Agent.Program.cs"));

                var dotnet = new Dotnet(tempDir);
                var result = await dotnet.Build();

                result.ThrowOnFailure("Failed to build intermediate project.");

                result = await dotnet.Pack($"/p:PackageId={name} /p:ToolCommandName={name} {projectFilePath} -o {options.OutputDirectory.FullName}");

                result.ThrowOnFailure("Package build failed.");
            }
        }
    }
}
