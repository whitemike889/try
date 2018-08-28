using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MLS.Agent.DotnetCli;
using MLS.Agent.Tools;

namespace WorkspaceServer.BuildLogParser
{
    public static class BuildLogParser
    {
        public static IEnumerable<string> FindCompilerCommandLine(this FileInfo logFile)
        {
            if (logFile == null)
            {
                throw new ArgumentNullException(nameof(logFile));
            }

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

            return null;
        }

        private static IEnumerable<string> RemoveDotnetAndCsc(this IEnumerable<string> args)
        {
            return args.Skip(2);
        }
    }
}
