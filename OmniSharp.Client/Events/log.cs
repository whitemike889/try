namespace OmniSharp.Client.Events
{
    public class log : AbstractOmniSharpEventBody
    {
        public log(string logLevel, string name, string message)
        {
            LogLevel = logLevel;
            Name = name;
            Message = message;
        }

        public string LogLevel { get; }
        public string Name { get; }
        public string Message { get; }
    }
}
