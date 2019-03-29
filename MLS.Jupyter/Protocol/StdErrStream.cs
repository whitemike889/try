namespace MLS.Jupyter.Protocol
{
    public class StdErrStream : Stream
    {
        public StdErrStream()
        {
            Name = "stderr";
        }
    }
}