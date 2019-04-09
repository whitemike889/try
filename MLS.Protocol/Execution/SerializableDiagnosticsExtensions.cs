using System.Collections.Generic;
using System.Linq;
using Microsoft.DotNet.Try.Protocol.Diagnostics;

namespace Microsoft.DotNet.Try.Protocol.Execution
{
    internal static class SerializableDiagnosticsExtensions
    {
        public static IOrderedEnumerable<SerializableDiagnostic> Sort(this IEnumerable<SerializableDiagnostic> source) =>
            source.OrderBy(d => d?.BufferId?.ToString())
                  .ThenBy(d => d.Start)
                  .ThenBy(d => d.End);
    }
}