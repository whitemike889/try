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
using Workspace = WorkspaceServer.Models.Execution.Workspace;

namespace WorkspaceServer.Servers.InMemory
{
    public class InMemoryWorkspaceServer : ILanguageService
    {
        private readonly ImmutableDictionary<string, InMemoryWorkspace> workspaces;
        private readonly BufferInliningTransformer _transformer =new BufferInliningTransformer();

        public InMemoryWorkspaceServer()
        {
            var builder = ImmutableDictionary.CreateBuilder<string, InMemoryWorkspace>();
            builder.Add("console",
                        new InMemoryWorkspace(
                            "console",
                            WorkspaceUtilities.DefaultReferencedAssemblies));
            builder.Add("script",
                        new InMemoryWorkspace(
                            "script",
                            WorkspaceUtilities.DefaultReferencedAssemblies));
            workspaces = builder.ToImmutableDictionary();
        }

        public async Task<CompletionResult> GetCompletionList(WorkspaceRequest request, Budget budget)
        {
            var workspace = workspaces[request.Workspace.WorkspaceType];

            var processed = await _transformer.TransformAsync(request.Workspace, budget);
            var viewPorts = _transformer.ExtractViewPorts(processed);
            var sourceFiles = processed.GetSourceFiles();
            var (compilation, documents) = await workspace.WithSources(sourceFiles);

            var file = processed.GetFileFromBufferId(request.ActiveBufferId);
            var (line, column, absolutePosition) = processed.GetTextLocation(request.ActiveBufferId, request.Position);
            Document selectedDocument = documents.First(doc => doc.Name == file.Name);

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

            var completionItems = await Task.WhenAll(
                                      completionList.Items
                                                    .Select(item => item.ToModel(symbolToSymbolKey, selectedDocument)));

            return new CompletionResult(completionItems
                                        .Deduplicate()
                                        .ToArray());
        }

        public async Task<DiagnosticResult> GetDiagnostics(Workspace request, Budget budget)
        {
            var workspace = workspaces[request.WorkspaceType];

            var processed = await _transformer.TransformAsync(request, budget);
            var viewPorts = _transformer.ExtractViewPorts(processed);
            var sourceFiles = processed.GetSourceFiles();
            var (compilation, _) = await workspace.WithSources(sourceFiles);

            var diagnostics = await ServiceHelpers.GetDiagnostics(request, compilation);
            return new DiagnosticResult(diagnostics.Select(e => e.Diagnostic).ToArray());
        }

        public async Task<SignatureHelpResponse> GetSignatureHelp(WorkspaceRequest request, Budget budget)
        {
            var (code, line, column, _) = await TransformWorkspaceAndPreparePositionalRequest(request, budget);

            var workspace = workspaces[request.Workspace.WorkspaceType];

            Workspace processed = await _transformer.TransformAsync(request.Workspace, budget);

            var viewPorts = _transformer.ExtractViewPorts(processed);
            var sourceFiles = processed.GetSourceFiles();
            var (compilation, documents) = await workspace.WithSources(sourceFiles);

            var requestActiveBufferId = request.ActiveBufferId.Split("@").First();

            var document = documents.FirstOrDefault(doc => doc.Name == requestActiveBufferId) 
                           ??
                           (documents.Count() == 1 ? documents.Single() : null);

            var tree = await document.GetSyntaxTreeAsync();

            var absolutePosition = processed.GetAbsolutePosition(
                request.ActiveBufferId, 
                request.Position);

            var syntaxNode = tree.GetRoot().FindToken(absolutePosition).Parent;

            return await SignatureHelpService.GetSignatureHelp(
                       () => Task.FromResult(compilation.GetSemanticModel(tree)),
                       syntaxNode,
                       absolutePosition);
        }

        private async Task<(string code, int line, int column, int absolutePosition)> TransformWorkspaceAndPreparePositionalRequest(WorkspaceRequest request, Budget budget)
        {
            var workspace = await _transformer.TransformAsync(request.Workspace, budget);

            var code = workspace.GetFileFromBufferId(request.ActiveBufferId).Text;
            // line and colum are 0 based

            var (line, column, absolutePosition) = workspace.GetTextLocation(request.ActiveBufferId, request.Position);

            return (code, line, column, absolutePosition);
        }
    }
}
