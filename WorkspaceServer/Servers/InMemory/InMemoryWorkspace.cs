using System.Linq;
using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkspaceServer.Models.Execution;
using Microsoft.CodeAnalysis.Host.Mef;
using System.IO;
using System.Threading;
using Clockwise;

namespace WorkspaceServer.Servers.InMemory
{
    public class InMemoryWorkspace
    {
        private readonly string name;
        private readonly IEnumerable<MetadataReference> additionalReferences;
       
        public InMemoryWorkspace(String name, MLS.Agent.Tools.Workspace workspace, IEnumerable<MetadataReference> additionalReferences)
        {
            this.name = name ?? throw new ArgumentNullException(nameof(name));
            Workspace = workspace;
            this.additionalReferences = additionalReferences ?? throw new ArgumentNullException(nameof(additionalReferences));
        }

        public MLS.Agent.Tools.Workspace Workspace { get; }

        public async Task<(Compilation, IEnumerable<Document>)> WithSources(IReadOnlyCollection<SourceFile> sources, Budget budget)
        {
            var workspace = new AdhocWorkspace(MefHostServices.DefaultHost);
            var compilationOptions = new CSharpCompilationOptions(OutputKind.ConsoleApplication);
            var projectId = ProjectId.CreateNewId();

            var projectInfo = ProjectInfo.Create(
                projectId,
                version: VersionStamp.Create(),
                name: "ScriptProject",
                assemblyName: "ScriptProject",
                language: LanguageNames.CSharp,
                compilationOptions: compilationOptions,
                metadataReferences: additionalReferences);

            workspace.AddProject(projectInfo);

            var currentSolution = workspace.CurrentSolution;

            foreach (var source in sources)
            {
                var docId = DocumentId.CreateNewId(projectId, "ScriptDocument");

                currentSolution = currentSolution.AddDocument(docId, source.Name, source.Text);

            }

            var project = currentSolution.GetProject(projectId);

            var newCompilation = await project.GetCompilationAsync().CancelIfExceeds(budget); 
        
            return (newCompilation, project.Documents);
        }
    }
}
