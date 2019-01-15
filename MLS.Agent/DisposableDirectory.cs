using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MLS.Agent
{
    public class DisposableDirectory : IDisposable
    {
        public DisposableDirectory(DirectoryInfo directory)
        {
            Directory = directory;
        }

        public static DisposableDirectory Create()
        {
            var tempDir = System.IO.Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
            return new DisposableDirectory(tempDir);

        }

        public DirectoryInfo Directory { get; }

        public void Dispose()
        {
            Directory.Delete(recursive: true);
        }
    }
}
