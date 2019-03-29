using MLS.Jupyter.Protocol;

namespace MLS.Jupyter
{
    public interface IMessageSender
    {
        bool Send(Message message);
    }
}