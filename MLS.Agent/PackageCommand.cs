using System;
using System.CommandLine;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using MLS.Agent.Tools;
using WorkspaceServer;

namespace MLS.Agent
{
    public class PackageCommand
    {
        public static Task Do(DirectoryInfo packTarget, IConsole console)
        {
            return Do(packTarget, packTarget, console);
        }

        public static async Task Do(DirectoryInfo packTarget, DirectoryInfo outputDirectory, IConsole console)
        {
            console.Out.WriteLine($"Creating package-tool from {packTarget.FullName}");

            using (var disposableDirectory = DisposableDirectory.Create())
            {
                var tempDir = disposableDirectory.Directory;
                var archivePath = Path.Combine(tempDir.FullName, "packagey.zip");

                ZipFile.CreateFromDirectory(packTarget.FullName, archivePath);
                console.Out.WriteLine(archivePath);

                var files = packTarget.GetFiles();
                var csproj = files.Single(f => f.Extension.Contains("csproj"));
                var name = Path.GetFileNameWithoutExtension(csproj.Name);

                var projectFilePath = Path.Combine(tempDir.FullName, "package-tool.csproj");
                var contentFilePath = Path.Combine(tempDir.FullName, "program.cs");

                await File.WriteAllTextAsync(
                    projectFilePath, 
                    FixCsproj(ReadManifestResource("MLS.Agent.MLS.PackageTool.csproj")));

                await File.WriteAllTextAsync(contentFilePath, ReadManifestResource("MLS.Agent.Program.cs"));

                var dotnet = new Dotnet(tempDir);
                var result = await dotnet.Build();

                result.ThrowOnFailure("Failed to build intermediate project.");

                result = await dotnet.Pack($"/p:PackageId={name} /p:ToolCommandName={name} {projectFilePath} -o {outputDirectory.FullName}");

                result.ThrowOnFailure("Package build failed.");
            }
        }

        private static string FixCsproj(string v)
        {
            return v.Replace("<!--", "").Replace("-->", "");
        }

        private static string ReadManifestResource(string resourceName)
        {
            var assembly = typeof(Program).Assembly;
            using (var reader = new StreamReader(assembly.GetManifestResourceStream(resourceName)))
            { 
                return reader.ReadToEnd();
            }
        }
    }
}
