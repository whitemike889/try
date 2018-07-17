using System;

namespace WorkspaceServer.Models.Completion
{
    public class CompletionResult
    {
        public CompletionItem[] Items { get; }

        public CompletionResult(CompletionItem[] items)
        {
            Items = items ?? Array.Empty<CompletionItem>();
        }
    }
}
