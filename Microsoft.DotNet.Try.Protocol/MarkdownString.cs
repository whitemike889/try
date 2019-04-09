namespace Microsoft.DotNet.Try.Protocol
{
    public class MarkdownString
    {
        public string Value { get; }
        public bool IsTrusted { get; }

        public MarkdownString(string value, bool isTrusted = false)
        {
            Value = value ?? "";
            IsTrusted = isTrusted;
        }

        public static implicit operator MarkdownString(string  value)  
        {
            return new MarkdownString(value);
        }
    }
}