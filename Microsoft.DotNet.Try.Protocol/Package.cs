namespace Microsoft.DotNet.Try.Protocol
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