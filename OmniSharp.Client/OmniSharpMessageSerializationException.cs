using System;

namespace OmniSharp.Client
{
    public class OmniSharpMessageSerializationException : Exception
    {
        public OmniSharpMessageSerializationException(string message) : base(message)
        {
        }

        public OmniSharpMessageSerializationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}