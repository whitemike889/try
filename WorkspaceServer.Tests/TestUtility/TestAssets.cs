using System.IO;

namespace WorkspaceServer.Tests.TestUtility
{
    public static class TestAssets
    {
        public static DirectoryInfo SampleConsole => 
            new DirectoryInfo(Path.Combine(GetTestProjectsFolder(), "SampleConsole"));

        private static string GetTestProjectsFolder()
        {
            var current = Directory.GetCurrentDirectory();
            return Path.Combine(current, "TestProjects");
        }
    }
}
