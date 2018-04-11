using System;
using System.Collections.Generic;
using System.Linq;
using OmniSharp.Client.Commands;

namespace WorkspaceServer.Models.Completion
{
    internal static class CompletionUtilities
    {
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

            var transformed = source.Select(item => new CompletionItem(
                item.DisplayText,
                item.Kind,
                item.CompletionText,
                item.CompletionText,
                item.CompletionText,
                item.Description));

            return new CompletionResult(transformed.ToArray());

        }
    }
}
