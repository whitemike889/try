using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MLS.Agent.Tools;

namespace WorkspaceServer
{
    public static class BuildLogParser
    {
        public static string[] FindCompilerCommandLineAndSetLanguageversion(this FileInfo logFile, string languageVersion)
        {
            if (logFile == null)
            {
                throw new ArgumentNullException(nameof(logFile));
            }

            var compilerCommandLine = GetCompilerCommandLine(logFile).ToList(); 
            compilerCommandLine.Add($"-langversion:{languageVersion}");
            return compilerCommandLine.ToArray();
        }

        private static IEnumerable<string> GetCompilerCommandLine(this FileInfo logFile)
        {
            var dotnetPath = DotnetMuxer.Path.FullName;

            using (var reader = logFile.OpenText())
            {
                string line = "";

                while ((line = reader.ReadLine()) != null)
                {
                    line = line.TrimStart();

                    if (line.StartsWith(dotnetPath, StringComparison.OrdinalIgnoreCase))
                    {
                        return line.Tokenize().RemoveDotnetAndCsc();
                    }
                }
            }

            throw new InvalidOperationException($"Compiler args not found in {logFile.FullName}.");
        }

        private static IEnumerable<string> RemoveDotnetAndCsc(this IEnumerable<string> args)
        {
            var foundCscDll = false;

            foreach (var arg in args)
            {
                if (foundCscDll)
                {
                    yield return arg;
                }
                else
                {
                    if (arg.EndsWith("csc.dll"))
                    {
                        foundCscDll = true;
                    }
                }
            }
        }
    }
}
