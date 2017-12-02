using System;

namespace OmniSharp.Client
{
    public class EventSerializationException : Exception
    {
        public EventSerializationException(string message) : base(message)
        {
        }
    }
}