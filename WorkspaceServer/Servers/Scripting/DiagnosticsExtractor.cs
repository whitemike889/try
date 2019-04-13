using System.Linq;
using System.Threading.Tasks;
using Clockwise;
using Microsoft.CodeAnalysis;
using Microsoft.DotNet.Try.Protocol;
using Microsoft.DotNet.Try.Protocol.Execution;
using WorkspaceServer.Transformations;
using Workspace = Microsoft.DotNet.Try.Protocol.Execution.Workspace;

namespace WorkspaceServer.Servers.Scripting
{
    internal static class DiazgnosticsExtractor
    {
        public static async Task<SerializableDiagnostic[]> ExtractSerializableDiagnosticsFromDocument(BufferId bufferId, Budget budget, Document selectedDocument, Workspace workspace)
        {
            var semanticModel = await selectedDocument.GetSemanticModelAsync();
            return ExtractSerializableDiagnosticsFromSemanticModel(bufferId, budget, semanticModel, workspace);
        }

        public static SerializableDiagnostic[] ExtractSerializableDiagnosticsFromSemanticModel(BufferId bufferId, Budget budget, SemanticModel semanticModel, Workspace workspace)
        {
            return null;
            // var diagnostics = workspace.MapDiagnostics(bufferId, semanticModel.GetDiagnostics().ToArray(), budget);
            // return diagnostics;
        }
    }
}