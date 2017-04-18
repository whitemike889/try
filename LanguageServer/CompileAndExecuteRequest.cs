namespace LanguageServer
{
    public class CompileAndExecuteRequest
    {
        private readonly string source;

        public CompileAndExecuteRequest(string source)
        {
            this.source = source ?? "";
        }

        public string Language => "csharp";

        public string[] Sources => new[] { source };
    }
}
