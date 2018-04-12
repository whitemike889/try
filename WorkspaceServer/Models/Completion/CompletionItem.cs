namespace WorkspaceServer.Models.Completion
{
    public class CompletionItem
    {
        public string DisplayText { get; }
        public string Kind { get; }
        public string FilterText { get; }
        public string SortText { get; }
        public string InsertText { get; }
        public string Documentation { get; set; }
        public CompletionItem(string displayText, string kind, string filterText, string sortText, string insertText, string documentation)
        {
            DisplayText = displayText;
            Kind = kind;
            FilterText = filterText;
            SortText = sortText;
            InsertText = insertText;
            Documentation = documentation;
        }

        public override string ToString() => DisplayText;
    }
}
