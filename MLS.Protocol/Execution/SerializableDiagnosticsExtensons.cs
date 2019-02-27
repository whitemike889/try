using System.Collections.Generic;
using System.Linq;
using MLS.Protocol.Diagnostics;

namespace MLS.Protocol.Execution
{
    internal static class SerializableDiagnosticsExtensons
    {
        public static IOrderedEnumerable<SerializableDiagnostic> Sort(this IEnumerable<SerializableDiagnostic> source) =>
            source.OrderBy(d => d?.BufferId?.ToString())
                  .ThenBy(d => d.Start)
                  .ThenBy(d => d.End);
    }
}