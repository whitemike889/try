using System.Collections.Generic;
using Newtonsoft.Json;

namespace OmniSharp.Client.Commands
{
    public abstract class AbstractOmniSharpFileCommandBody : AbstractOmniSharpCommandBody
    {
        public int Line { get; set; }

        public int Column { get; set; }

        public string Buffer { get; set; }

        public IEnumerable<LinePositionSpanTextChange> Changes { get; set; }

        public string FileName { get; set; }
    }

    public abstract class AbstractOmniSharpCommandBody : IOmniSharpCommandBody
    {
        [JsonIgnore]
        public abstract string Command { get; }
    }
}
