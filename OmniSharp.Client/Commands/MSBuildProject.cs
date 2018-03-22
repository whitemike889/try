using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OmniSharp.Client.Commands
{
    public class MSBuildProject
    {
        public MSBuildProject(
            string projectGuid,
            string path,
            string assemblyName,
            string targetPath,
            string targetFramework,
            IEnumerable<string> sourceFiles,
            string outputPath,
            bool isExe,
            bool isUnityProject)
        {
            ProjectGuid = projectGuid;
            Path = path;
            AssemblyName = assemblyName;
            TargetPath = targetPath;
            TargetFramework = targetFramework;
            SourceFiles = sourceFiles?.Select(f => new FileInfo(f)) ??
                          Array.Empty<FileInfo>();
            OutputPath = outputPath;
            IsExe = isExe;
            IsUnityProject = isUnityProject;
        }

        public string ProjectGuid { get; }
        public string Path { get; }
        public string AssemblyName { get; }
        public string TargetPath { get; }
        public string TargetFramework { get; }
        public IEnumerable<FileInfo> SourceFiles { get; }
        public string OutputPath { get; }
        public bool IsExe { get; }
        public bool IsUnityProject { get; }
    }
}