using System.IO;

namespace MLS.Agent.CommandLine
{
    public class VerifyOptions
    {
        public VerifyOptions(DirectoryInfo dir)
        {
            Dir = dir;
        }

        public DirectoryInfo Dir { get; }
    }
}