using System.IO;
using System.Runtime.CompilerServices;
using Recipes;
using WorkspaceServer.Models.Execution;

namespace WorkspaceServer.Tests
{
    public static class Create
    {
        public static DirectoryInfo TestFolder([CallerMemberName] string testName = null)
        {
            var existingFolders = MLS.Agent.Tools.Workspace.DefaultWorkspacesDirectory.GetDirectories($"{testName}.*");

            return MLS.Agent.Tools.Workspace.DefaultWorkspacesDirectory.CreateSubdirectory($"{testName}.{existingFolders.Length + 1}");
        }

        public static MLS.Agent.Tools.Workspace TestWorkspace([CallerMemberName] string testName = null) =>
            MLS.Agent.Tools.Workspace.Copy(Default.TemplateWorkspace, testName);

        public static Workspace SimpleRunRequest(
            string consoleOutput = "Hello!",
            string workspaceType = null) =>
            new Workspace(SimpleConsoleAppCodeWithoutNamespaces(consoleOutput), workspaceType: workspaceType);

        public static string SimpleRunRequestJson(
            string consoleOutput = "Hello!",
            string workspaceType = null)
        {
            return new
            {
                buffer = SimpleConsoleAppCodeWithoutNamespaces(consoleOutput),
                workspaceType
            }.ToJson();
        }

        public static string SimpleConsoleAppCodeWithoutNamespaces(string consoleOutput)
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
