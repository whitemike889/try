using System;
using System.Runtime.CompilerServices;
using MLS.Agent.Tools;
using Recipes;
using WorkspaceServer.Models.Execution;

namespace WorkspaceServer.Tests
{
    public static class Create
    {
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
