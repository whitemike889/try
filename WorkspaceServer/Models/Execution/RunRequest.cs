using System;
using System.Collections.Generic;

namespace WorkspaceServer.Models.Execution
{
    public class RunRequest
    {
        public string Language { get; }

        public string RawSource { get; }

        public string[] Sources { get; }

        public string[] Usings { get; }

        public RunRequest(string source, string[] usings = null)
        {
            Language = "csharp";
            RawSource = source;
            Sources = GetSources(RawSource);
            Usings = usings ?? Array.Empty<string>();
        }

        public IEnumerable<SourceFile> GetSourceFiles()
        {
            for (var i = 0; i < Sources.Length; i++)
            {
                yield return SourceFile.Create(Sources[i],
                                               i == 0
                                                   ? "Program.cs"
                                                   : $"{i + 1}.cs");
            }
        }

        private static string[] GetSources(string source)
            => IsFragment(source)
                   ? new[] { ProgramCs, ScaffoldedSnippet(source) }
                   : new[] { source };

        private static bool IsFragment(string source)
            => source != null && !source.Contains("public static void Main(");

        private static string ScaffoldedSnippet(string source)
            => $"{UsingStatements} public static class Fragment {{ public static void Invoke() {{ {source} }} }}";

        private const string UsingStatements = @"
using System; using System.Collections.Generic; using System.Linq;";

        private const string ProgramCs = UsingStatements + @"
public class Program
{
    public static void Main() {
        Fragment.Invoke();
    }
}";
    }
}
