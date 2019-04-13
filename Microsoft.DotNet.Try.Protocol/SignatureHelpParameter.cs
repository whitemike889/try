namespace Microsoft.DotNet.Try.Protocol
{
    public class SignatureHelpParameter
    {
        public string Name { get; set; }

        public string Label { get; set; }

        public MarkdownString Documentation { get; set; }
    }
}