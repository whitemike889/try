using Microsoft.DotNet.Try.Jupyter.Protocol;

namespace Microsoft.DotNet.Try.Jupyter
{
    public interface IMessageSender
    {
        bool Send(Message message);
    }
}