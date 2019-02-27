using System.IO;

namespace MLS.Agent.Tests.TestUtility
{
    public static class TestAssets
    {
        public static DirectoryInfo SampleConsole
        {
            get => new DirectoryInfo(Path.Combine(GetTestProjectsFolder(), "SampleConsole"));
        }

        private static string GetTestProjectsFolder()
        {
            var current = Directory.GetCurrentDirectory();
            return Path.Combine(current, "TestProjects");
        }
    }
}
