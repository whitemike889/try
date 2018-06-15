using System;
using System.Linq;
using System.Threading.Tasks;
using Clockwise;
using Microsoft.CodeAnalysis;
using WorkspaceServer.Models;
using WorkspaceServer.Transformations;
using Workspace = WorkspaceServer.Models.Execution.Workspace;

namespace WorkspaceServer.Servers.Roslyn
{
    internal static class ServiceHelpers
    {
        internal static async Task<(SerializableDiagnostic Diagnostic, string ErrorMessage)[]> GetDiagnostics(
            Workspace workspace,
            Compilation compilation,
            Budget budget = null)
        {
            budget = budget ?? new Budget();

            var processor = new BufferInliningTransformer();
            var processed = await processor.TransformAsync(workspace, budget);
            var viewPorts = processor.ExtractViewPorts(processed);
            var sourceDiagnostics = compilation.GetDiagnostics().Where(d => d.Id != "CS7022");
            budget.RecordEntry();
            return DiagnosticTransformer.ReconstructDiagnosticLocations(
                                            sourceDiagnostics,
                                            viewPorts,
                                            BufferInliningTransformer.PaddingSize)
                                        .ToArray();
        }
    }
}
