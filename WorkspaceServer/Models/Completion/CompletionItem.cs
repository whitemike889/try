namespace WorkspaceServer.Models.Completion
{
    public class CompletionItem
    {
        public string DisplayText { get; }
        public string Kind { get; }
        public string FilterText { get; }
        public string SortText { get; }

        public CompletionItem(string displayText, string kind, string filterText, string sortText)
        {
            DisplayText = displayText;
            Kind = kind;
            FilterText = filterText;
            SortText = sortText;
        }

        public override string ToString() => DisplayText;
    }
}
