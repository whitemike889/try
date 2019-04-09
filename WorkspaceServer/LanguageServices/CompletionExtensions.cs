using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Tags;
using Microsoft.DotNet.Try.Protocol;
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
            WellKnownTags.Class,
            WellKnownTags.Constant,
            WellKnownTags.Delegate,
            WellKnownTags.Enum,
            WellKnownTags.EnumMember,
            WellKnownTags.Event,
            WellKnownTags.ExtensionMethod,
            WellKnownTags.Field,
            WellKnownTags.Interface,
            WellKnownTags.Intrinsic,
            WellKnownTags.Keyword,
            WellKnownTags.Label,
            WellKnownTags.Local,
            WellKnownTags.Method,
            WellKnownTags.Module,
            WellKnownTags.Namespace,
            WellKnownTags.Operator,
            WellKnownTags.Parameter,
            WellKnownTags.Property,
            WellKnownTags.RangeVariable,
            WellKnownTags.Reference,
            WellKnownTags.Structure,
            WellKnownTags.TypeParameter);

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

        public static Microsoft.DotNet.Try.Protocol.Completion.CompletionItem ToModel(this CompletionItem item, Dictionary<(string, int), ISymbol> recommendedSymbols,
            Document document)
        {
            var documentation =  GetDocumentation(item, recommendedSymbols, document);

            return new Microsoft.DotNet.Try.Protocol.Completion.CompletionItem(
                displayText: item.DisplayText,
                kind: item.GetKind(),
                filterText: item.FilterText,
                sortText: item.SortText,
                insertText: item.FilterText,
                documentation: documentation);
        }

        public static MarkdownString GetDocumentation(this CompletionItem item, Dictionary<(string, int), ISymbol> recommendedSymbols,
        Document document)
        {
            var symbol = GetCompletionSymbolAsync(item, recommendedSymbols, document);
            if (symbol != null)
            {
                var xmlDocumentation = symbol.GetDocumentationCommentXml();
                return DocumentationConverter.GetDocumentation(symbol, "\n");
            }

            return null;
        }

        public static  ISymbol GetCompletionSymbolAsync(
            CompletionItem completionItem, Dictionary<(string, int), ISymbol> recommendedSymbols,
            Document document)
        {
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
