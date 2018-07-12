using System;
using System.Linq;
using Clockwise;
using Microsoft.CodeAnalysis;
using WorkspaceServer.Models;
using WorkspaceServer.Transformations;
using Workspace = WorkspaceServer.Models.Execution.Workspace;

namespace WorkspaceServer.Servers.Roslyn
{
    internal static class ServiceHelpers
    {
        internal static SerializableDiagnostic[] GetDiagnostics(
            Workspace workspace,
            Compilation compilation,
            Budget budget = null)
        {
            budget = budget ?? new Budget();

            var processor = new BufferInliningTransformer();
            var viewPorts = processor.ExtractViewPorts(workspace);
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
