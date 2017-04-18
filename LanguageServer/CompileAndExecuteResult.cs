namespace LanguageServer
{
    public class CompileAndExecuteResult
    {
        public bool Succeeded { get; set; }

        public string Reason { get; set; }

        public string Phase { get; set; }

        public string[] Output { get; set; }
    }
}
