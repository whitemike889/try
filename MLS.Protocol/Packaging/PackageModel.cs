namespace MLS.Protocol.Packaging
{
    public class Package
    {
        public Package(bool isBlazorSupported)
        {
            IsBlazorSupported = isBlazorSupported;
        }

        public bool IsBlazorSupported { get; }
    }
}