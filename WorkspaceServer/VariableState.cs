using System;

namespace WorkspaceServer
{
    public class VariableState
    {
        public VariableState(int lineNumber, object value, Type type)
        {
            LineNumber = lineNumber;
            Value = value;
            Type = type;
        }

        public int LineNumber { get; }

        public object Value { get; }
        public Type Type { get; }
    }
}