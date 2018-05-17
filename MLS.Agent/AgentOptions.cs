namespace MLS.Agent
{
    public class AgentOptions
    {
        public bool IsLanguageServiceMode { get; }

        public AgentOptions(bool isLanguageServiceMode)
        {
            IsLanguageServiceMode = isLanguageServiceMode;
        }
    }
}