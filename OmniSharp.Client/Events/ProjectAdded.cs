using System;

namespace OmniSharp.Client.Events
{
    public class ProjectAdded : AbstractOmniSharpEventBody
    {
        public Guid ProjectGuid { get; set; }
        public string Path { get; set; }
        public string AssemblyName { get; set; }
        public string TargetPath { get; set; }
        public string TargetFramework { get; set; }
        public string[] SourceFiles { get; set; }
        public string TargetFrameworks { get; set; }
        public string Name { get; set; }
        public string FriendlyName { get; set; }
        public string ShortName { get; set; }
        public string OutputPath { get; set; }
        public bool IsExe { get; set; }
        public bool IsUnityProject { get; set; }
    }
}
