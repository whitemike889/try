using System.Collections.Generic;
using System.Linq;

namespace Microsoft.DotNet.Try.Protocol
{
    public static class CompletionUtilities
    {
        public static IEnumerable<CompletionItem> Deduplicate(this IEnumerable<CompletionItem> source)
        {
            return source.Distinct(CompletionItemEqualityComparer.Instance);
        }

        private class CompletionItemEqualityComparer : IEqualityComparer<CompletionItem>
        {
            private CompletionItemEqualityComparer()
            {
            }

            public static CompletionItemEqualityComparer Instance { get; } = new CompletionItemEqualityComparer();

            public bool Equals(CompletionItem x, CompletionItem y)
            {
                return x.Kind.Equals(y.Kind) &&
                       x.InsertText.Equals(y.InsertText);
            }

            public int GetHashCode(CompletionItem obj)
            {
                return (obj.Kind + obj.InsertText).GetHashCode();
            }
        }
    }
}
