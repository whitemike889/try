using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Recipes;

namespace WorkspaceServer.Tests
{
    public static class Create
    {
        public static DirectoryInfo TestFolder([CallerMemberName] string testName = null)
        {
            var existingFolders = MLS.Agent.Tools.Workspace.DefaultWorkspacesDirectory.GetDirectories($"{testName}.*");

            return MLS.Agent.Tools.Workspace.DefaultWorkspacesDirectory.CreateSubdirectory($"{testName}.{existingFolders.Length + 1}");
        }

        public static async Task<Workspace> TestWorkspace([CallerMemberName] string testName = null)
        {
            var workspace = new Workspace(Workspace.CreateDirectory(testName), "test");

            await workspace.EnsureCreated();
            await workspace.EnsureBuilt();

            return workspace;
        }

        public static WorkspaceRunRequest SimpleRunRequest(
            string consoleOutput = "Hello!",
            string workspaceType = null) =>
            new WorkspaceServer.Models.Execution.Workspace(SimpleConsoleAppCodeWithoutNamespaces(consoleOutput), workspaceType: workspaceType);

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
