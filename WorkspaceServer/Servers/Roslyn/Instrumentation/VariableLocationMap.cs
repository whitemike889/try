using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace WorkspaceServer.Servers.Roslyn.Instrumentation
{
    public class VariableLocationMap : ISerializableOnce
    {
        public Dictionary<ISymbol, HashSet<VariableLocation>> Data;
        public VariableLocationMap()
        {
            Data = new Dictionary<ISymbol, HashSet<VariableLocation>>();
        }

        public void AddLocations(ISymbol variable, IEnumerable<VariableLocation> locations)
        {
            if (!Data.ContainsKey(variable))
            {
                Data[variable] = new HashSet<VariableLocation>();
            }

            foreach (var location in locations)
            {
                Data[variable].Add(location);
            }
        }
        public string Serialize()
        {
            var strings = Data.Select(kv =>
            {
                var variable = kv.Key;
                return SerializeForKey(variable);
            });
            var joined = @"\""variableLocations\"": [" + strings.Join() + "]";
            return joined;
        }

        public string SerializeForKey(ISymbol key)
        {
            string varLocations = Data[key]
                .Select(locations => locations.Serialize())
                .Join();
            var declaringSpan = key.DeclaringSyntaxReferences.First().Span;
            string output = $@"
{{
    \""name\"": \""{key.Name}\"",
    \""locations\"": [{varLocations}],
    \""declaredAt\"": {{
        \""start\"": {declaringSpan.Start},
        \""end\"": {declaringSpan.End}
    }}
}}
";
            return output;
        }
    }
}
