using System;
using System.Collections.Generic;
using System.Linq;

namespace WorkspaceServer.Models.Execution
{
    public class Variable
    {
        private readonly Dictionary<int, VariableState> states = new Dictionary<int, VariableState>();

        public Variable(string name) => Name = name ?? throw new ArgumentNullException(nameof(name));

        public string Name { get; }

        public IReadOnlyCollection<VariableState> States => states.Values;

        public object Value => states.OrderBy(s => s.Value.LineNumber).Select(s => s.Value?.Value).LastOrDefault();

        internal void TryAddState(VariableState state) =>
            states[state.LineNumber] = state;
    }
}