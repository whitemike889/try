using System;
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

        [ImportingConstructor]
        public EmitService(OmniSharpWorkspace workspace, ILoggerFactory loggerFactory)
        {
            _workspace = workspace;

            loggerFactory.CreateLogger<EmitService>()
                         .LogInformation("Loaded plugin {plugin}", this);
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

            if (!diagnostics.Any(e => e.Severity == DiagnosticSeverity.Error))
            {
                compilation.Emit(project.OutputFilePath);
            }

            return new EmitResponse
            {
                Diagnostics = diagnostics,
                OutputAssemblyPath = project.OutputFilePath
            };
        }
    }
}
