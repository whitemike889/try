using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;

namespace WorkspaceServer.Servers.Local
{
    public class ProjectWorkspaceServer
    {
        private readonly AdhocWorkspace _workspace = new AdhocWorkspace(MefHostServices.DefaultHost);

        private readonly DocumentId _documentId;

        public ProjectWorkspaceServer(
            IEnumerable<string> defaultUsings,
            MetadataReference[] metadataReferences)
        {
            if (defaultUsings == null)
            {
                throw new ArgumentNullException(nameof(defaultUsings));
            }

            if (metadataReferences == null)
            {
                throw new ArgumentNullException(nameof(metadataReferences));
            }

            var projectId = ProjectId.CreateNewId("ScriptProject");

            var compilationOptions = new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                usings: defaultUsings);

            var projectInfo = ProjectInfo.Create(
                projectId,
                version: VersionStamp.Create(),
                name: "ScriptProject",
                assemblyName: "ScriptProject",
                language: LanguageNames.CSharp,
                compilationOptions: compilationOptions,
                metadataReferences: metadataReferences);

            _workspace.AddProject(projectInfo);

            _documentId = DocumentId.CreateNewId(projectId, "ScriptDocument");

            var documentInfo = DocumentInfo.Create(_documentId,
                                                   name: "ScriptDocument");

            _workspace.AddDocument(documentInfo);
        }

        public Document ForkDocument(string text)
        {
            var document = _workspace.CurrentSolution.GetDocument(_documentId);
            return document.WithText(SourceText.From(text));
        }








    }

  


}
