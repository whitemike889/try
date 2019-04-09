using Microsoft.DotNet.Try.Jupyter.Protocol;

namespace Microsoft.DotNet.Try.Jupyter
{
    public interface IMessageBuilder
    {
        Header CreateHeader(string messageType, string session);
        Message CreateMessage(string messageType, object content, Header parentHeader);
    }
}