namespace MLS.Jupyter.Protocol
{
    public class StdOutStream : Stream
    {
        public StdOutStream()
        {
            Name = "stdout";
        }
    }
}