using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Clockwise;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using MLS.Project.Execution;
using MLS.Project.Extensions;
using MLS.Protocol.Execution;
using WorkspaceServer.Servers.Roslyn.Instrumentation;
using WorkspaceServer.Packaging;
using static System.Environment;
using Workspace = MLS.Protocol.Execution.Workspace;

namespace WorkspaceServer.Servers.Roslyn
{
    public static class WorkspaceBuildExtensions
    {
        public static async Task<Compilation> Compile(
            this Package build, 
            Workspace workspace, 
            Budget budget, 
            BufferId activeBufferId)
        {
            await build.EnsureReady(budget);

            var sourceFiles = workspace.GetSourceFiles().ToArray();

            var (compilation, documents) = await build.GetCompilation(sourceFiles, SourceCodeKind.Regular, workspace.Usings, budget);

            var viewports = workspace.ExtractViewPorts();

            var diagnostics = compilation.GetDiagnostics();

            if (workspace.IncludeInstrumentation && !diagnostics.ContainsError())
            {
                var activeDocument = GetActiveDocument(documents, activeBufferId);
                compilation = await AugmentCompilationAsync(viewports, compilation, activeDocument, activeBufferId, build);
            }

            return compilation;
        }

        private static async Task<Compilation> AugmentCompilationAsync(
            IEnumerable<Viewport> viewports,
            Compilation compilation,
            Document document,
            BufferId activeBufferId,
            Package build)
        {
            var regions = InstrumentationLineMapper.FilterActiveViewport(viewports, activeBufferId)
                .Where(v => v.Destination?.Name != null)
                .GroupBy(v => v.Destination.Name,
                         v => v.Region,
                        (name, region) => new InstrumentationMap(name, region)
            );

            var solution = document.Project.Solution;
            var newCompilation = compilation;
            foreach (var tree in newCompilation.SyntaxTrees)
            {
                var replacementRegions = regions?.Where(r => tree.FilePath.EndsWith(r.FileToInstrument)).FirstOrDefault()?.InstrumentationRegions;

                var subdocument = solution.GetDocument(tree);
                var visitor = new InstrumentationSyntaxVisitor(subdocument, await subdocument.GetSemanticModelAsync(), replacementRegions);
                var linesWithInstrumentation = visitor.Augmentations.Data.Keys;

                var activeViewport = viewports.DefaultIfEmpty(null).First();

                var (augmentationMap, variableLocationMap) =
                    await InstrumentationLineMapper.MapLineLocationsRelativeToViewportAsync(
                        visitor.Augmentations,
                        visitor.VariableLocations,
                        document,
                        activeViewport);

                var rewrite = new InstrumentationSyntaxRewriter(
                    linesWithInstrumentation,
                    variableLocationMap,
                    augmentationMap);
                var newRoot = rewrite.Visit(tree.GetRoot());
                var newTree = tree.WithRootAndOptions(newRoot, tree.Options);

                newCompilation = newCompilation.ReplaceSyntaxTree(tree, newTree);
            }

            var instrumentationSyntaxTree = await build.GetInstrumentationEmitterSyntaxTree();
            newCompilation = newCompilation.AddSyntaxTrees(instrumentationSyntaxTree);

            var augmentedDiagnostics = newCompilation.GetDiagnostics();
            if (augmentedDiagnostics.ContainsError())
            {
                throw new InvalidOperationException(
                    $@"Augmented source failed to compile

Diagnostics
-----------
{string.Join(NewLine, augmentedDiagnostics)}

Source
------
{newCompilation.SyntaxTrees.Select(s => $"// {s.FilePath ?? "(anonymous)"}{NewLine}//---------------------------------{NewLine}{NewLine}{s}").Join(NewLine + NewLine)}");
            }

            return newCompilation;
        }

        public static async Task<(Compilation compilation, IReadOnlyCollection<Document> documents)> GetCompilation(
            this Package build,
            IReadOnlyCollection<SourceFile> sources,
            SourceCodeKind sourceCodeKind,
            IEnumerable<string> defaultUsings,
            Budget budget)
        {
            var projectId = ProjectId.CreateNewId();

            var workspace = await build.CreateRoslynWorkspace(projectId);

            var currentSolution = workspace.CurrentSolution;

            foreach (var source in sources)
            {
                if (currentSolution.Projects
                                   .SelectMany(p => p.Documents)
                                   .FirstOrDefault(d => d.IsMatch(source)) is Document document)
                {
                    // there's a pre-existing document, so overwrite its contents
                    document = document.WithText(source.Text);
                    document = document.WithSourceCodeKind(sourceCodeKind);
                    currentSolution = document.Project.Solution;
                }
                else
                {
                    var docId = DocumentId.CreateNewId(projectId, $"{build.Name}.Document");

                    currentSolution = currentSolution.AddDocument(docId, source.Name, source.Text);
                    currentSolution = currentSolution.WithDocumentSourceCodeKind(docId, sourceCodeKind);
                }
            }

            var project = currentSolution.GetProject(projectId);
            var options = (CSharpCompilationOptions)project.CompilationOptions;
            project = project.WithCompilationOptions(options.WithUsings(defaultUsings));

            var compilation = await project.GetCompilationAsync().CancelIfExceeds(budget);

            return (compilation, project.Documents.ToArray());
        }

        private static Document GetActiveDocument(IEnumerable<Document> documents, BufferId activeBufferId)
        {
            return documents.First(d => d.Name.Equals(activeBufferId.FileName));
        }
    }
}

