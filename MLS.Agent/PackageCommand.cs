using Microsoft.ApplicationInsights.AspNetCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
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

            var csproj = directory.GetFiles().Single(f => f.Extension.Contains("csproj"));
            var name = Path.GetFileNameWithoutExtension(csproj.Name);

            string csprojName = $"package-tool.csproj";
            var projectFilePath = Path.Combine(tempDir.FullName, csprojName);

            var assembly = typeof(PackageCommand).Assembly;
            var names = assembly.GetManifestResourceNames();

            Console.WriteLine(string.Join("\n", names));

            await File.WriteAllTextAsync(projectFilePath, ReadStream(assembly, "MLS.Agent.MLS.PackageTool.csproj"));

            var contentFilePath = Path.Combine(tempDir.FullName, "program.cs");
            await File.WriteAllTextAsync(contentFilePath, ReadStream(assembly, "MLS.Agent.Program.cs"));




            var dotnet = new Dotnet(tempDir);
            var result = await dotnet.Build();
            Console.WriteLine(string.Join("\n", result.Output.Concat(result.Error)));

            result = await dotnet.Pack($"/p:PackageId={name} /p:ToolCommandName={name} {projectFilePath}");
            Console.WriteLine(string.Join("\n", result.Output.Concat(result.Error)));

        }

        private static string ReadStream(Assembly a, string resourceName)
        {
            using (var reader = new StreamReader(a.GetManifestResourceStream(resourceName)))
            { 
                return reader.ReadToEnd();
            }
        }
    }
}
