
using System;
using System.Linq;
using System.Threading.Tasks;
using Clockwise;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using WorkspaceServer.Models;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Transformations;

namespace WorkspaceServer.Servers.Roslyn
{
    internal static class ServiceHelpers
    {
        internal async static Task<(SerializableDiagnostic Diagnostic, string ErrorMessage)[]> GetDiagnostics(
            Models.Execution.Workspace workspace,
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
