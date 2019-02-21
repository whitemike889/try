namespace MLS.WasmCodeRunner
{
    public class InteropMessage<T>
    {
        public InteropMessage(int sequence, T data)
        {
            this.sequence = sequence;
            this.data = data;
        }

        public int sequence { get; set; }
        public T data { get; set; }
    }
}
