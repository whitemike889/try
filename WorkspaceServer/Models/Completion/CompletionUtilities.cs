using System;
using System.Collections.Generic;
using System.Linq;
using OmniSharp.Client.Commands;

namespace WorkspaceServer.Models.Completion
{
    internal static class CompletionUtilities
    {
        private sealed class CompletionTextKindEqualityComparer : IEqualityComparer<AutoCompleteResponse>
        {
            public bool Equals(AutoCompleteResponse x, AutoCompleteResponse y)
            {
                if (ReferenceEquals(x, y))
                    return true;
                if (ReferenceEquals(x, null))
                    return false;
                if (ReferenceEquals(y, null))
                    return false;
                if (x.GetType() != y.GetType())
                    return false;
                return string.Equals(x.CompletionText, y.CompletionText) && string.Equals(x.Kind, y.Kind);
            }

            public int GetHashCode(AutoCompleteResponse obj)
            {
                unchecked
                {
                    return ((obj.CompletionText != null ? obj.CompletionText.GetHashCode() : 0) * 397) ^ (obj.Kind != null ? obj.Kind.GetHashCode() : 0);
                }
            }
        }

        public static IEqualityComparer<AutoCompleteResponse> CompletionTextKindComparer { get; } = new CompletionTextKindEqualityComparer();

        public static string GetWordAt(this string source, int position)
        {
            var index = position;
            while (index >= 1)
            {
                var ch = source[index - 1];
                if (ch != '_' && !char.IsLetterOrDigit(ch))
                {
                    break;
                }

                index--;
            }

            return source.Substring(index, position - index);
        }

        public static CompletionResult ToCompletionResult(this IEnumerable<AutoCompleteResponse> items)
        {
            var source = items ?? Array.Empty<AutoCompleteResponse>();

            var transformed = source.Distinct(CompletionTextKindComparer).Select(item => new CompletionItem(
                displayText: item.CompletionText,
                kind: item.Kind,
                filterText: item.CompletionText,
                insertText: item.CompletionText,
                sortText: item.CompletionText,
                documentation: DocumentationConverter.ConvertDocumentation(item.Description,"\n")));

            return new CompletionResult(transformed.ToArray());

        }
    }
}
