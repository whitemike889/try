using System;
using System.IO;
using WorkspaceServer.Servers.Local;

namespace WorkspaceServer
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
                            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                            ".trydotnet",
                            "projects"));
        }

        public Project(string name) : this(new DirectoryInfo(Path.Combine(DefaultProjectsDirectory.FullName, name)))
        {
        }

        public Project(DirectoryInfo directory)
        {
            Directory = directory ?? throw new ArgumentNullException(nameof(directory));
        }

        public DirectoryInfo Directory { get; }

        public static DirectoryInfo DefaultProjectsDirectory { get; }

        public void IfEmptyInitializeFromDotnetTemplate(string template)
        {
            if (!Directory.Exists)
            {
                Directory.Create();
            }

            if (Directory.GetFiles().Length == 0)
            {
                new Dotnet(Directory).New(template);
            }
        }
    }
}
