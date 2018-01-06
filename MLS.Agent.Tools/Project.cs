using System;
using System.IO;
using Pocket;
using static Pocket.Logger<MLS.Agent.Tools.Project>;

namespace MLS.Agent.Tools
{
    public class Project
    {
        static Project()
        {
            var omnisharpPathEnvironmentVariableName = "TRYDOTNET_PROJECTS_PATH";

            var environmentVariable = Environment.GetEnvironmentVariable(omnisharpPathEnvironmentVariableName);

            DefaultProjectsDirectory =
                environmentVariable != null
                    ? new DirectoryInfo(environmentVariable)
                    : new DirectoryInfo(
                        Path.Combine(
                            Paths.UserProfile,
                            ".trydotnet",
                            "projects"));

            Log.Info("Projects path is {DefaultProjectsDirectory}", DefaultProjectsDirectory);
        }

        public Project(string name) : this(
            new DirectoryInfo(Path.Combine(DefaultProjectsDirectory.FullName, name)),
            name)
        {
        }

        public Project(DirectoryInfo directory, string name = null)
        {
            Name = name ?? directory.Name;
            Directory = directory ?? throw new ArgumentNullException(nameof(directory));
        }

        private bool IsDirectoryCreated { get; set; }

        public bool IsCreated { get; private set; }

        public bool IsBuilt { get; private set; }

        public DirectoryInfo Directory { get; }

        public string Name { get; }

        public static DirectoryInfo DefaultProjectsDirectory { get; }

        public void EnsureCreated(string template, bool build = false)
        {
            if (!IsDirectoryCreated)
            {
                Directory.Refresh();

                if (!Directory.Exists)
                {
                    Directory.Create();
                    Directory.Refresh();
                }

                IsDirectoryCreated = true;
            }

            if (!IsCreated)
            {
                if (Directory.GetFiles().Length == 0)
                {
                    var dotnet = new Dotnet(Directory);
                    dotnet
                        .New(template, args: $"--name \"{Name}\" --output \"{Directory.FullName}\"")
                        .ThrowOnFailure();

                    if (build)
                    {
                        EnsureBuilt();
                    }
                }

                IsCreated = true;
            }
        }

        public void EnsureBuilt()
        {
            if (!IsBuilt)
            {
                if (Directory.GetFiles("*.deps.json", SearchOption.AllDirectories).Length == 0)
                {
                    new Dotnet(Directory).Build().ThrowOnFailure();
                }

                IsBuilt = true;
            }
        }

        public static Project Copy(
            Project fromProject,
            string folderName = null)
        {
            if (fromProject == null)
            {
                throw new ArgumentNullException(nameof(fromProject));
            }

            folderName = folderName ?? fromProject.Name;

            DirectoryInfo destination;
            var i = 0;

            do
            {
                destination = new DirectoryInfo(
                    Path.Combine(
                        fromProject
                            .Directory
                            .Parent
                            .FullName,
                        $"{folderName}.{++i}"));
            } while (destination.Exists);

            fromProject.Directory.CopyTo(destination);

            var copy = new Project(destination,
                                   folderName ?? fromProject.Name);

            copy.IsCreated = fromProject.IsCreated;
            copy.IsBuilt = fromProject.IsBuilt;
            copy.IsDirectoryCreated = true;

            return copy;
        }
    }
}
