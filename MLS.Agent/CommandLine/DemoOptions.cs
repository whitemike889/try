using System.IO;

namespace MLS.Agent.CommandLine
{
    public class DemoOptions
    {
        public DemoOptions(DirectoryInfo output, bool enablePreviewFeatures = false)
        {
            Output = output;
            EnablePreviewFeatures = enablePreviewFeatures;
        }

        public DirectoryInfo Output { get;  }
        public bool EnablePreviewFeatures { get; set; }
    }
}