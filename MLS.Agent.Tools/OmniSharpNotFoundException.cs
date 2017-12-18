using System;

namespace WorkspaceServer.Servers.OmniSharp
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