using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using MLS.Protocol;
using WorkspaceServer.Models;

namespace WorkspaceServer.Servers.Scripting
{
    public static class CompletionExtensions
    {
        private static readonly string SymbolCompletionProvider = "Microsoft.CodeAnalysis.CSharp.Completion.Providers.SymbolCompletionProvider";
        private static readonly string Provider = nameof(Provider);
        private static readonly string SymbolName = nameof(SymbolName);
        private static readonly string Symbols = nameof(Symbols);
        private static readonly string GetSymbolsAsync = nameof(GetSymbolsAsync);

        private static readonly ImmutableArray<string> KindTags = ImmutableArray.Create(
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
            foreach (var tag in KindTags)
            {
                if (completionItem.Tags.Contains(tag))
                {
                    return tag;
                }
            }

            return null;
        }

        public static async Task<MLS.Protocol.Completion.CompletionItem> ToModel(this CompletionItem item, Dictionary<(string, int), ISymbol> recommendedSymbols,
            Document document)
        {
            var documentation = await GetDocumentation(item, recommendedSymbols, document);

            return new MLS.Protocol.Completion.CompletionItem(
                displayText: item.DisplayText,
                kind: item.GetKind(),
                filterText: item.FilterText,
                sortText: item.SortText,
                insertText: item.FilterText,
                documentation: documentation);
        }

        public static async Task<MarkdownString> GetDocumentation(this CompletionItem item, Dictionary<(string, int), ISymbol> recommendedSymbols,
        Document document)
        {
            var symbol = await GetCompletionSymbolAsync(item, recommendedSymbols, document);
            if (symbol != null)
            {
                var xmlDocumentation = symbol.GetDocumentationCommentXml();
                return DocumentationConverter.GetDocumentation(symbol, "\n");
            }

            return null;
        }

        public static async Task<ISymbol> GetCompletionSymbolAsync(
            CompletionItem completionItem, Dictionary<(string, int), ISymbol> recommendedSymbols,
            Document document)
        {
            await Task.Yield();
            var properties = completionItem.Properties;

            if (properties.TryGetValue(Provider, out var provider) && provider == SymbolCompletionProvider)
            {
                if (recommendedSymbols.TryGetValue((properties[SymbolName], int.Parse(properties[nameof(SymbolKind)])), out var symbol))
                {
                    // We were able to match this SymbolCompletionProvider item with a recommended symbol
                    return symbol;
                }
            }

            return null;
        }
    }
}
