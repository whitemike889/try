using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using WorkspaceServer;

namespace MLS.Agent
{
    public class PackageCommand
    {
        public static async Task Do(DirectoryInfo directory)
        {
            Console.WriteLine($"Creating package-tool from {directory.FullName}");
            var tempDir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));

            var archiveName = "packagey.zip";
            var archivePath = Path.Combine(tempDir.FullName, archiveName);

            ZipFile.CreateFromDirectory(directory.FullName, archivePath);
            Console.WriteLine(archivePath);

            var csproj = directory.GetFiles().Single(f => f.Extension.Contains("csproj"));
            var name = Path.GetFileNameWithoutExtension(csproj.Name);

            string csprojName = $"package-tool.csproj";
            var projectFilePath = Path.Combine(tempDir.FullName, csprojName);
            var contentFilePath = Path.Combine(tempDir.FullName, "program.cs");

            await File.WriteAllTextAsync(projectFilePath, ReadManifestResource("MLS.Agent.MLS.PackageTool.csproj"));
            await File.WriteAllTextAsync(contentFilePath, ReadManifestResource("MLS.Agent.Program.cs"));

            var dotnet = new Dotnet(tempDir);
            var result = await dotnet.Build();
            if (result.ExitCode != 0)
            {
                throw new Exception("Failed to build intermediate project");
            }

            var outputLocation = directory.FullName;
            result = await dotnet.Pack($"/p:PackageId={name} /p:ToolCommandName={name} {projectFilePath} -o {outputLocation}");
            if (result.ExitCode != 0)
            {
                throw new Exception("Package build failed");
            }
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
