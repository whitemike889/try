using System;
using System.IO;

namespace MLS.Agent.CommandLine
{
    public class PackOptions
    {
        public PackOptions(DirectoryInfo packTarget, DirectoryInfo outputDirectory = null) 
        {
            PackTarget = packTarget ?? throw new ArgumentNullException(nameof(packTarget));
            OutputDirectory = outputDirectory ?? packTarget;
        }

        public DirectoryInfo PackTarget { get; }

        public DirectoryInfo OutputDirectory { get; }
    }
}