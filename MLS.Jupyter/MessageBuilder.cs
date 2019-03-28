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
    internal class MessageBuilder : IMessageBuilder
    {
        public Header CreateHeader(string messageType, string session)
        {
            var newHeader = new Header
            {
                Username = Constants.USERNAME,
                Session = session,
                MessageId = Guid.NewGuid().ToString(),
                MessageType = messageType,
                Date = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                Version = "5.3"
            };

            return newHeader;
        }

        public Message CreateMessage(string messageType, object content, Header parentHeader)
        {
            var session = parentHeader.Session;

            var message = new Message
            {
                ParentHeader = parentHeader,
                Header = CreateHeader(messageType, session),
                Content = content
            };

            return message;
        }
    }
}