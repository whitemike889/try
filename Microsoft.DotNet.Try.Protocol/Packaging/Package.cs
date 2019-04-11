namespace Microsoft.DotNet.Try.Protocol.Packaging
{
    public class Package
    {
        public bool IsBlazorSupported { get; }

        public Package(bool isBlazorSupported)
        {
            IsBlazorSupported = isBlazorSupported;
        }
    }
}