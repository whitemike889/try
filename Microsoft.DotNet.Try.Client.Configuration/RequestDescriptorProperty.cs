using System;

namespace Microsoft.DotNet.Try.Client.Configuration
{
    public class RequestDescriptorProperty
    {
        public string Name { get; }
        public object Value { get; }

        public RequestDescriptorProperty(string name, object value = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            Name = name;
            Value = value;
        }
    }
}