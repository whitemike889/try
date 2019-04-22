using System;
using System.IO;

namespace Microsoft.DotNet.Try.Markdown
{
    public class RelativeDirectoryPath :
        RelativePath,
        IEquatable<RelativeDirectoryPath>
    {
        public RelativeDirectoryPath(string path)
        {
            Value = NormalizeDirectory(path);
            ThrowIfPathIsRooted(Value);
        }

        private void ThrowIfPathIsRooted(string path)
        {
            if (Path.IsPathRooted(path))
            {
                throw new ArgumentException($"Path cannot be absolute: {path}");
            }
        }

        public bool Equals(RelativeDirectoryPath other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((RelativeDirectoryPath) obj);
        }
    }
}