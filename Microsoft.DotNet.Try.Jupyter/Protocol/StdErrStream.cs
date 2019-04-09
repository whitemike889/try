namespace Microsoft.DotNet.Try.Jupyter.Protocol
{
    public class StdErrStream : Stream
    {
        public StdErrStream()
        {
            Name = "stderr";
        }
    }
}