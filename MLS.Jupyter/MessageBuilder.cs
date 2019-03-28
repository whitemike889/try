using System;
using MLS.Jupyter.Protocol;

namespace MLS.Jupyter
{
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