using System.Collections.Generic;

namespace LanguageServer
{
    public class ProcessResult
    {
        public ProcessResult(
            bool succeeded,
            IReadOnlyCollection<string> output)
        {
            Succeeded = succeeded;
            Output = output;
        }

        public bool Succeeded { get; }

        public IReadOnlyCollection<string> Output { get; }
    }
}
