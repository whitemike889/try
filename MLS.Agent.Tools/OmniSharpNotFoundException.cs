using System;

namespace MLS.Agent.Tools
{
    public class OmniSharpNotFoundException : Exception
    {
        public OmniSharpNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public OmniSharpNotFoundException(string message) : base(message)
        {
        }
    }
}