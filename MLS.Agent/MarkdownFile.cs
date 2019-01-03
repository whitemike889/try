using System;
using System.IO;

namespace MLS.Agent
{
    public class MarkdownFile
    {
        public FileInfo FileInfo { get; }

        public MarkdownFile(FileInfo fileInfo)
        {
            if(fileInfo == null)
            {
                throw new ArgumentNullException(nameof(fileInfo));
            }

            if(!fileInfo.Exists)
            {
                throw new FileNotFoundException("The specified file doesnot exist", fileInfo.FullName);
            }

            FileInfo = fileInfo;
        }

        public bool TryGetContent(out string content)
        {
            content = null;
            if (FileInfo.Exists)
            {
                content = File.ReadAllText(FileInfo.FullName);
                return true;
            }

            return false;
        }
    }
}
