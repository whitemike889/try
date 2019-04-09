using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Clockwise;
using Microsoft.CodeAnalysis;
using Microsoft.DotNet.Try.Protocol.Diagnostics;
using Microsoft.DotNet.Try.Protocol.Execution;
using WorkspaceServer.Transformations;
using Workspace = Microsoft.DotNet.Try.Protocol.Execution.Workspace;

namespace WorkspaceServer
{
    internal static class DiagnosticsExtractor
    {
        public static async Task<IReadOnlyCollection<SerializableDiagnostic>> ExtractSerializableDiagnosticsFromDocument(
            BufferId bufferId,
            Budget budget,
            Document selectedDocument,
            Workspace workspace)
        {
            var semanticModel = await selectedDocument.GetSemanticModelAsync();
            return ExtractSerializableDiagnosticsFromSemanticModel(bufferId, budget, semanticModel, workspace);
        }

        public static IReadOnlyCollection<SerializableDiagnostic> ExtractSerializableDiagnosticsFromSemanticModel(
            BufferId bufferId,
            Budget budget,
            SemanticModel semanticModel,
            Workspace workspace)
        {
            var diagnostics = workspace.MapDiagnostics(bufferId, semanticModel.GetDiagnostics().ToArray(), budget);
            return diagnostics.DiagnosticsInActiveBuffer;
        }
    }
}