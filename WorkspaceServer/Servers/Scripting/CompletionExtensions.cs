using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using WorkspaceServer.Models;

namespace WorkspaceServer.Servers.Scripting
{
    internal static class CompletionExtensions
    {
        private static readonly MethodInfo _getSymbolsAsync;
        private static readonly string SymbolCompletionProvider = "Microsoft.CodeAnalysis.CSharp.Completion.Providers.SymbolCompletionProvider";
        private static readonly string Provider = nameof(Provider);
        private static readonly string SymbolName = nameof(SymbolName);
        private static readonly string Symbols = nameof(Symbols);
        private static readonly string SymbolCompletionItem = "Microsoft.CodeAnalysis.Completion.Providers.SymbolCompletionItem";
        private static readonly string GetSymbolsAsync = nameof(GetSymbolsAsync);

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

        public static async Task<Models.Completion.CompletionItem> ToModel(this CompletionItem item, Dictionary<(string, int), ISymbol> recommendedSymbols,
            Document document)
        {
            var documentation = await GetDocumentation(item, recommendedSymbols, document);
            return new Models.Completion.CompletionItem(
                displayText: item.DisplayText,
                kind: item.GetKind(),
                filterText: item.FilterText,
                sortText: item.SortText,
                insertText: item.FilterText,
                documentation: documentation);
        }

        public static async Task<string> GetDocumentation(this CompletionItem item, Dictionary<(string, int), ISymbol> recommendedSymbols,
        Document document)
        {
            var documentation = string.Empty;
            var symbol = await GetCompletionSymbolAsync(item, recommendedSymbols, document);
            if (symbol != null)
            {
                var xmlDocumentation = symbol.GetDocumentationCommentXml();
                documentation = DocumentationConverter.ConvertDocumentation(xmlDocumentation, "\n");
            }

            return documentation;
        }
        public static async Task<ISymbol> GetCompletionSymbolAsync(
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

            // if the completion provider encoded symbols into Properties, we can return them
            if (properties.ContainsKey(Symbols))
            {
                // the API to decode symbols is not public at the moment
                // http://source.roslyn.io/#Microsoft.CodeAnalysis.Features/Completion/Providers/SymbolCompletionItem.cs,93
                var decodedSymbolsTask = (Task<ImmutableArray<ISymbol>>)_getSymbolsAsync.Invoke(null, new object[] { completionItem, document, default(CancellationToken) });
                if (decodedSymbolsTask != null)
                {
                    var symbols = await decodedSymbolsTask;
                    return symbols.FirstOrDefault();
                }
            }

            return null;
        }
    }

   
}
