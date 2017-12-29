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
                            Paths.UserProfile(),
                            ".trydotnet",
                            "projects"));

            Log.Info("Projects path is {DefaultProjectsDirectory}", DefaultProjectsDirectory);
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
    }
}
