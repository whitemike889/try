using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkspaceServer.Servers.Roslyn.Instrumentation
{
    public static class EnumerableExtensions
    {
        public static string Join<T>(this IEnumerable<T> seq, string separator = ",")
        {
            return String.Join(separator, seq);
        }
    }
}
