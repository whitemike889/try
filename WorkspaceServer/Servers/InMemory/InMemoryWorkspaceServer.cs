using System;
using System.Threading.Tasks;
using Clockwise;
using WorkspaceServer.Models;
using WorkspaceServer.Models.Completion;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Models.SingatureHelp;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using WorkspaceServer.Servers.Roslyn;
using System.Linq;
using WorkspaceServer.Transformations;
using WorkspaceServer.Servers.Scripting;
using Microsoft.CodeAnalysis.Recommendations;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Completion;

namespace WorkspaceServer.Servers.InMemory
{
    public class InMemoryWorkspaceServer : ILanguageService
    {
        ImmutableDictionary<string, InMemoryWorkspace> workspaces;

        public InMemoryWorkspaceServer()
        {
            var builder = ImmutableDictionary.CreateBuilder<string, InMemoryWorkspace>();
            builder.Add("console", new InMemoryWorkspace("console", additionalReferences: new MetadataReference[] { }));
            builder.Add("script", new InMemoryWorkspace("script", additionalReferences: new MetadataReference[] { }));
            workspaces = builder.ToImmutableDictionary();
        }

        public async Task<CompletionResult> GetCompletionList(WorkspaceRequest request, Budget budget)
        {
            var workspace = workspaces[request.Workspace.WorkspaceType];

            var processor = new BufferInliningTransformer();
            var processed = await processor.TransformAsync(request.Workspace, budget);
            var viewPorts = processor.ExtractViewPorts(processed);
            var sourceFiles = processed.GetSourceFiles();
            var (compilation, documents) = workspace.WithSources(sourceFiles);

            var file = request.Workspace.GetFileFromBufferId(request.ActiveBufferId);
            var (line, column, absolutePosition) = request.Workspace.GetTextLocation(request.ActiveBufferId, request.Position);
            Document selectedDocument = documents.Where(doc => doc.Name == request.ActiveBufferId).First();

            var service = CompletionService.GetService(selectedDocument);
            var completionList = await service.GetCompletionsAsync(selectedDocument, absolutePosition);
            var semanticModel = await selectedDocument.GetSemanticModelAsync();
            var symbols = await Recommender.GetRecommendedSymbolsAtPositionAsync(semanticModel, request.Position, selectedDocument.Project.Solution.Workspace);

            var symbolToSymbolKey = new Dictionary<(string, int), ISymbol>();
            foreach (var symbol in symbols)
            {
                var key = (symbol.Name, (int)symbol.Kind);
                if (!symbolToSymbolKey.ContainsKey(key))
                {
                    symbolToSymbolKey[key] = symbol;
                }
            }

            var items = completionList.Items.Select(item => item.ToModel(symbolToSymbolKey, selectedDocument).Result).ToArray();

            return new CompletionResult(items: items);
        }

        public async Task<DiagnosticResult> GetDiagnostics(Models.Execution.Workspace request, Budget budget)
        {
            var workspace = workspaces[request.WorkspaceType];

            var processor = new BufferInliningTransformer();
            var processed = await processor.TransformAsync(request, budget);
            var viewPorts = processor.ExtractViewPorts(processed);
            var sourceFiles = processed.GetSourceFiles();
            var (compilation, _) = workspace.WithSources(sourceFiles);

            var diagnostics = await ServiceHelpers.GetDiagnostics(request, compilation);
            return new DiagnosticResult(diagnostics.Select(e => e.Diagnostic).ToArray());
        }

        public async Task<SignatureHelpResponse> GetSignatureHelp(WorkspaceRequest request, Budget budget)
        {
            var workspace = workspaces[request.Workspace.WorkspaceType];

            var processor = new BufferInliningTransformer();
            var processed = await processor.TransformAsync(request.Workspace, budget);
            var viewPorts = processor.ExtractViewPorts(processed);
            var sourceFiles = processed.GetSourceFiles();
            var (compilation, documents) = workspace.WithSources(sourceFiles);

            var document = documents.Where(doc => doc.Name == request.ActiveBufferId).First();
            var tree = await document.GetSyntaxTreeAsync();
            Func<Task<SemanticModel>> getSemanticModel = () => Task.FromResult(compilation.GetSemanticModel(tree));

            return await SignatureHelpService.GetSignatureHelp(getSemanticModel, tree.GetRoot(), request.Position);
        }
    }
}
