using System.IO;

namespace MLS.Agent.CommandLine
{
    public class DemoOptions
    {
        public DemoOptions(DirectoryInfo output)
        {
            Output = output;
        }

        public DirectoryInfo Output { get;  }
    }
}