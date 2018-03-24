using Newtonsoft.Json;

namespace OmniSharp.Client.Commands
{
    public abstract class AbstractOmniSharpCommandArguments : IOmniSharpCommandArguments
    {
        [JsonIgnore]
        public abstract string Command { get; }
    }
}
