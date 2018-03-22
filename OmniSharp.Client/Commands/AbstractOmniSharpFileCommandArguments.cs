using System;
using System.Collections.Generic;
using System.IO;

namespace OmniSharp.Client.Commands
{
    public abstract class AbstractOmniSharpFileCommandArguments : AbstractOmniSharpCommandArguments
    {
        protected AbstractOmniSharpFileCommandArguments(
            FileInfo fileName = null,
            string buffer = null,
            int? line = null,
            int? column = null,
            IEnumerable<LinePositionSpanTextChange> changes = null)
        {
            Line = line;
            Column = column;
            Buffer = buffer;
            Changes = changes;
            FileName = fileName?.FullName;
        }

        public int? Line { get; }

        public int? Column { get; }

        public string Buffer { get; }

        public IEnumerable<LinePositionSpanTextChange> Changes { get; }

        public string FileName { get; }
    }
}
