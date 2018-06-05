using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using OmniSharp.Mef;
using Diagnostic = MLS.Agent.Tools.Diagnostic;

namespace OmniSharp.Emit
{
    [OmniSharpHandler(EndpointName, LanguageNames.CSharp)]
    public class EmitService : IRequestHandler<EmitRequest, EmitResponse>
    {
        public const string EndpointName = "/emit";

        private readonly OmniSharpWorkspace _workspace;

        private readonly ILogger<EmitService> _logger;

        [ImportingConstructor]
        public EmitService(OmniSharpWorkspace workspace, ILoggerFactory loggerFactory)
        {
            _workspace = workspace;

            _logger = loggerFactory.CreateLogger<EmitService>();
            _logger.LogInformation("Loaded plugin {plugin}", this);
        }

        public async Task<EmitResponse> Handle(EmitRequest request)
        {
            var project = _workspace.CurrentSolution.Projects.SingleOrDefault();

            if (project == null)
            {
                throw new InvalidOperationException($"Command '{EndpointName}' is not valid without a project.");
            }

            var compilation = await project.GetCompilationAsync();

            var diagnostics = compilation.GetDiagnostics()
                                         .Select(e => new Diagnostic(e))
                                         .ToArray();

            if (diagnostics.All(e => e.Severity != DiagnosticSeverity.Error))
            {
                if (request.IncludeInstrumentation)
                {
                    _logger.LogDebug("Performing Instrumentation");
                    compilation = AugmentCompilation(request.InstrumentationRegions, compilation);
                }

                compilation.Emit(project.OutputFilePath);
            }

            return new EmitResponse
            {
                Diagnostics = diagnostics,
                OutputAssemblyPath = project.OutputFilePath
            };
        }

        private Compilation AugmentCompilation(IEnumerable<InstrumentationMap> regions, Compilation compilation)
        {
            var newCompilation = compilation;
            foreach (var tree in newCompilation.SyntaxTrees)
            {
                var replacementRegions = regions?.Where(r => tree.FilePath.EndsWith(r.FileToInstrument)).FirstOrDefault()?.InstrumentationRegions;

                var semanticModel = newCompilation.GetSemanticModel(tree);

                var visitor = new InstrumentationSyntaxVisitor(semanticModel, replacementRegions);
                var augmentations = visitor.GetAugmentations();

                var rewrite = new InstrumentationSyntaxRewriter(augmentations);
                var newRoot = rewrite.Visit(tree.GetRoot());
                var newTree = tree.WithRootAndOptions(newRoot, tree.Options);

                newCompilation = newCompilation.ReplaceSyntaxTree(tree, newTree);
            }

            // if it failed to compile, just return the original, unaugmented compilation
            var augmentedDiagnostics = newCompilation.GetDiagnostics();
            if (augmentedDiagnostics.Any(e => e.Severity == DiagnosticSeverity.Error))
            {
                _logger.LogError("Augmented source failed to compile: {0}", string.Join(Environment.NewLine, augmentedDiagnostics));
                return compilation;
            }

            return newCompilation;
        }
    }
}
