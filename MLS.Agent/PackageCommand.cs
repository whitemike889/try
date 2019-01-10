using Microsoft.ApplicationInsights.AspNetCore;
using System;
using System.Collections.Generic;
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
            //var dir = Environment.CurrentDirectory;
            Console.WriteLine($"Creating package-tool from {directory.FullName}");
            var tempDir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));

            var archiveName = "packagey.zip";
            var archivePath = Path.Combine(tempDir.FullName, archiveName);

            ZipFile.CreateFromDirectory(directory.FullName, archivePath);
            Console.WriteLine(archivePath);

            var csproj = directory.GetFiles().Single(f => f.Extension == "csproj");

            const string name = "package-tool";
            string csprojName = $"{name}.csproj";
            var projectFilePath = Path.Combine(tempDir.FullName, csprojName);
            await File.WriteAllTextAsync(projectFilePath, Resource.MLS_PackageTool);

            var contentFilePath = Path.Combine(tempDir.FullName, "program.cs");
            await File.WriteAllTextAsync(contentFilePath, Resource.Program);

            var dotnet = new Dotnet(tempDir);
            var result = await dotnet.Build();
            Console.WriteLine(string.Join("\n", result.Output.Concat(result.Error)));

            dotnet.Pack($"/p:PackageId={} {projectFilePath}")
            

        }
    }
}
