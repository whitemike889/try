using MLS.Jupyter.Protocol;

namespace MLS.Jupyter
{
    public interface IMessageBuilder
    {
        Header CreateHeader(string messageType, string session);
        Message CreateMessage(string messageType, object content, Header parentHeader);
    }
}