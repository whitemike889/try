using System.IO;

namespace MLS.Agent
{
    public class RelativeFilePath : RelativePath
    {
        private RelativeDirectoryPath _directory;

        public RelativeFilePath(string filePath) : base(filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new System.ArgumentException("File path cannot be null or empty", nameof(filePath));
            }
        }
        public RelativeDirectoryPath Directory
        {
            get
            {
                return _directory ?? (_directory = new RelativeDirectoryPath(Path.GetDirectoryName(Value)));
            }
        }

        public string Extension
        {
            get
            {
                return Path.GetExtension(Value);
            }
        }
    }
}
