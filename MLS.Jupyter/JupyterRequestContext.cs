using System;
using MLS.Jupyter.Protocol;

namespace MLS.Jupyter
{
    public class JupyterRequestContext
    {
        public IMessageBuilder Builder { get; }
        public IMessageSender ServerChannel { get; }
        public IMessageSender IoPubChannel { get; }
        public Message Request { get; }
        public IRequestHandlerStatus RequestHandlerStatus { get; }

        public JupyterRequestContext(IMessageBuilder messageBuilder, IMessageSender serverChannel, IMessageSender ioPubChannel, Message request,
            IRequestHandlerStatus requestHandlerStatus)
        {
            Builder = messageBuilder ?? throw new ArgumentNullException(nameof(messageBuilder));
            ServerChannel = serverChannel ?? throw new ArgumentNullException(nameof(serverChannel));
            IoPubChannel = ioPubChannel ?? throw new ArgumentNullException(nameof(ioPubChannel));
            Request = request ?? throw new ArgumentNullException(nameof(request));
            RequestHandlerStatus = requestHandlerStatus;
        }
    }
}