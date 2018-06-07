using System.Linq;
using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using WorkspaceServer.Servers.Roslyn;
using static WorkspaceServer.Models.Execution.Workspace;
using WorkspaceServer.Models.Execution;
using Microsoft.CodeAnalysis.Host.Mef;

namespace WorkspaceServer.Servers.InMemory
{
    public class InMemoryWorkspace
    {
        private readonly string name;
        private readonly IEnumerable<MetadataReference> additionalReferences;

        public InMemoryWorkspace(String name, IEnumerable<MetadataReference> additionalReferences)
        {
            this.name = name ?? throw new ArgumentNullException(nameof(name));
            this.additionalReferences = additionalReferences ?? throw new ArgumentNullException(nameof(additionalReferences));
        }

        public (Compilation, IEnumerable<Document>) WithSources(IEnumerable<SourceFile> sources)
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

            var compilation = CSharpCompilation.Create(name, references: additionalReferences);

            var documents = sources.Select(source =>
            {
                var docId = DocumentId.CreateNewId(projectId, "ScriptDocument");

                var documentInfo = DocumentInfo.Create(docId,
                    name: source.Name,
                    sourceCodeKind: SourceCodeKind.Regular);

                return workspace.AddDocument(documentInfo).WithText(source.Text);
            });

            var newCompilation = compilation.AddSyntaxTrees(sources.Select(source => CSharpSyntaxTree.ParseText(source.Text, path: source.Name)));
            return (newCompilation, documents);
        }
    }
}
