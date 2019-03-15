using System;
using System.IO;

namespace MLS.Agent.CommandLine
{
    public class PackOptions
    {
        public PackOptions(DirectoryInfo packTarget, DirectoryInfo outputDirectory = null, bool enableBlazor = false) 
        {
            PackTarget = packTarget ?? throw new ArgumentNullException(nameof(packTarget));
            OutputDirectory = outputDirectory ?? packTarget;
            EnableBlazor = enableBlazor;
        }

        public DirectoryInfo PackTarget { get; }

        public DirectoryInfo OutputDirectory { get; }
        public bool EnableBlazor { get; }
    }
}