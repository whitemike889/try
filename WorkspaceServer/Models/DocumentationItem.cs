namespace WorkspaceServer.Models
{
    public class DocumentationItem
    {
        public string Name { get; }
        public string Documentation { get; }
        public DocumentationItem(string name, string documentation)
        {
            Name = name;
            Documentation = documentation;
        }
    }
}
