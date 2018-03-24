using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Completion;

namespace WorkspaceServer.Servers.Scripting
{
    internal static class CompletionExtensions
    {
        private static readonly ImmutableArray<string> s_kindTags = ImmutableArray.Create(
            CompletionTags.Class,
            CompletionTags.Constant,
            CompletionTags.Delegate,
            CompletionTags.Enum,
            CompletionTags.EnumMember,
            CompletionTags.Event,
            CompletionTags.ExtensionMethod,
            CompletionTags.Field,
            CompletionTags.Interface,
            CompletionTags.Intrinsic,
            CompletionTags.Keyword,
            CompletionTags.Label,
            CompletionTags.Local,
            CompletionTags.Method,
            CompletionTags.Module,
            CompletionTags.Namespace,
            CompletionTags.Operator,
            CompletionTags.Parameter,
            CompletionTags.Property,
            CompletionTags.RangeVariable,
            CompletionTags.Reference,
            CompletionTags.Structure,
            CompletionTags.TypeParameter);

        public static string GetKind(this CompletionItem completionItem)
        {
            foreach (var tag in s_kindTags)
            {
                if (completionItem.Tags.Contains(tag))
                {
                    return tag;
                }
            }

            return null;
        }

        public static Models.Completion.CompletionItem ToModel(this CompletionItem item)
        {
            return new Models.Completion.CompletionItem(
                displayText: item.DisplayText,
                kind: item.GetKind(),
                filterText: item.FilterText,
                sortText: item.SortText);
        }
    }
}
