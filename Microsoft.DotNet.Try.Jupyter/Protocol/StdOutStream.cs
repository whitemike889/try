namespace Microsoft.DotNet.Try.Jupyter.Protocol
{
    public class StdOutStream : Stream
    {
        public StdOutStream()
        {
            Name = "stdout";
        }
    }
}