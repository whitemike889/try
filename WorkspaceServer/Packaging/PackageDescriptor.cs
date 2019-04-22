using System;

namespace WorkspaceServer.Packaging
{
    public class PackageDescriptor
    {
        public PackageDescriptor(
            string name, 
            string version = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            }

            Name = name;
            Version = version;
        }

        public string Name { get; }

        public string Version { get; }

        public override string ToString() => Name;
    }
}