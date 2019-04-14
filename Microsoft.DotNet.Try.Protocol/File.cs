namespace Microsoft.DotNet.Try.Protocol
{
    public class File
    {
        public File(string name, string text, int order = 0)
        {
            Name = name;
            Text = text;
            Order = order;
        }

        public string Name { get; }

        public string Text { get; }
        public int Order { get; }

        public override string ToString() => $"{nameof(File)}: {Name}";
    }
}