namespace WorkspaceServer
{
    public class PackageDescriptor
    {
        public PackageDescriptor(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}