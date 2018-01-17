using System;
using System.IO;
using System.Runtime.CompilerServices;
using MLS.Agent.Tools;
using Recipes;
using WorkspaceServer.Models.Execution;

namespace WorkspaceServer.Tests
{
    public static class Create
    {
        public static DirectoryInfo TestFolder([CallerMemberName] string testName = null)
        {
            var existingFolders = Workspace.DefaultWorkspacesDirectory.GetDirectories($"{testName}.*");

            return Workspace.DefaultWorkspacesDirectory.CreateSubdirectory($"{testName}.{existingFolders.Length + 1}");
        }

        public static Workspace TestWorkspace([CallerMemberName] string testName = null) =>
            Workspace.Copy(Default.TemplateWorkspace, testName);

        public static RunRequest SimpleRunRequest(
            string consoleOutput = "Hello!") =>
            new RunRequest(SampleCode(consoleOutput));

        public static string SimpleRunRequestJson(
            string consoleOutput = "Hello!") =>
            new
            {
                Source = SampleCode(consoleOutput)
            }.ToJson();

        private static string SampleCode(string consoleOutput)
        {
            return $@"
using System;

public static class Hello
{{
    public static void Main()
    {{
        Console.WriteLine(""{consoleOutput}"");
    }}
}}";
        }
    }
}
