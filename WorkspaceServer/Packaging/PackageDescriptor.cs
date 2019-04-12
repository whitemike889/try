namespace WorkspaceServer.Packaging
{
    public class PackageDescriptor
    {
        public PackageDescriptor(string name, bool isRebuildable = false)
        {
            Name = name;
            IsRebuildable = isRebuildable;
        }

        public string Name { get; }

        public bool IsRebuildable { get; }

        public override string ToString() => Name;
    }
}