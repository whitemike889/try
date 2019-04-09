using System;
using System.CommandLine;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using MLS.Agent.Tools;
using WorkspaceServer;
using WorkspaceServer.Packaging;

namespace MLS.Agent.CommandLine
{
    public static class PackCommand
    {
        public static async Task<string> Do(PackOptions options, IConsole console)
        {
            console.Out.WriteLine($"Creating package-tool from {options.PackTarget.FullName}");

            using (var disposableDirectory = DisposableDirectory.Create())
            {
                var temp = disposableDirectory.Directory;
                var temp_projects = temp.CreateSubdirectory("projects");

                string name = GetProjectFileName(options);

                var temp_projects_packtarget = temp_projects.CreateSubdirectory("packTarget");
                DirectoryCopy(options.PackTarget, temp_projects_packtarget.FullName, copySubDirs: true);

                if (options.EnableBlazor)
                {
                    string runnerDirectoryName = $"runner-{name}";
                    var temp_projects_blazorRunner = temp_projects.CreateSubdirectory(runnerDirectoryName);
                    var temp_projects_blazorRunner_mlsblazor = temp_projects_blazorRunner.CreateSubdirectory("MLS.Blazor");
                    await AddBlazorProject(temp_projects_blazorRunner_mlsblazor, GetProjectFile(temp_projects_packtarget), name);
                }

                var temp_toolproject = temp.CreateSubdirectory("project");
                var archivePath = Path.Combine(temp_toolproject.FullName, "package.zip");
                ZipFile.CreateFromDirectory(temp_projects.FullName, archivePath, CompressionLevel.Fastest, includeBaseDirectory: false);

                console.Out.WriteLine(archivePath);

                var projectFilePath = Path.Combine(temp_toolproject.FullName, "package-tool.csproj");
                var contentFilePath = Path.Combine(temp_toolproject.FullName, "program.cs");

                await File.WriteAllTextAsync(
                    projectFilePath,
                    typeof(Program).ReadManifestResource("MLS.Agent.MLS.PackageTool.csproj"));

                await File.WriteAllTextAsync(contentFilePath, typeof(Program).ReadManifestResource("MLS.Agent.Program.cs"));

                var dotnet = new Dotnet(temp_toolproject);
                var result = await dotnet.Build();

                result.ThrowOnFailure("Failed to build intermediate project.");
                var versionArg = "";

                if(!string.IsNullOrEmpty(options.Version))
                {
                    versionArg = $"/p:PackageVersion={options.Version}";
                }

                result = await dotnet.Pack($"/p:PackageId=dotnettry.{name} /p:ToolCommandName=dotnettry.{name} {versionArg} {projectFilePath} -o {options.OutputDirectory.FullName}");

                result.ThrowOnFailure("Package build failed.");

                return $"dotnettry.{name}";
            }
        }

        private static string GetProjectFileName(PackOptions options)
        {
            var csproj = GetProjectFile(options.PackTarget);
            var name = Path.GetFileNameWithoutExtension(csproj.Name);
            return name;
        }

        private static async Task AddBlazorProject(DirectoryInfo blazorTargetDirectory, FileInfo projectToReference, string name)
        {
            var initializer = new BlazorPackageInitializer(name, new System.Collections.Generic.List<string>());
            await initializer.Initialize(blazorTargetDirectory);

            await AddReference(blazorTargetDirectory, projectToReference);
        }

        private static async Task AddReference(DirectoryInfo blazorTargetDirectory, FileInfo projectToReference)
        {
            var dotnet = new Dotnet(blazorTargetDirectory);
            (await dotnet.AddReference(projectToReference)).ThrowOnFailure();
        }

        private static FileInfo GetProjectFile(DirectoryInfo directory)
        {
            return directory.GetFiles("*.csproj").Single();
        }

        private static void DirectoryCopy(DirectoryInfo source, string destination, bool copySubDirs)
        {

            if (!source.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + source.FullName);
            }

            DirectoryInfo[] dirs = source.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = source.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destination, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destination, subdir.Name);
                    DirectoryCopy(subdir, temppath, copySubDirs);
                }
            }
        }
    }
}
