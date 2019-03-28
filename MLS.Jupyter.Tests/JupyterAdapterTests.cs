using System;
using System.Text;
using Assent;
using MLS.Jupyter.Protocol;
using NetMQ;
using Xunit;
using Xunit.Abstractions;

namespace MLS.Jupyter.Tests
{

    public class JupyterMessageContractTests
    {
        class TextSocket : IOutgoingSocket
        {
            readonly StringBuilder _buffer = new StringBuilder();
            public bool TrySend(ref Msg msg, TimeSpan timeout, bool more)
            {
                var decoded = SendReceiveConstants.DefaultEncoding.GetString(msg.Data);
                _buffer.AppendLine($"data: {decoded} more: {more}");
                return true;
            }

            public string GetEncodedMessage()
            {
                return _buffer.ToString();
            }
        }

        private readonly Configuration _configuration;

        public JupyterMessageContractTests()
        {
            _configuration = new Configuration()
                .UsingExtension("json");

            _configuration = _configuration.SetInteractive(true);
        }

        [Fact]
        public void KernelInfoReply_contract_has_not_been_broken()
        {
            var socket = new TextSocket();
            var sender = new MessageSender(socket, new SignatureValidator("key", "HMACSHA256"));
            var kernelInfoReply = new KernelInfoReply
            {
                ProtocolVersion = "5.3",
                Implementation = ".NET",
                ImplementationVersion = "0.0.3",
                LanguageInfo = new LanguageInfo
                {
                    Name = "C#",
                    Version = typeof(string).Assembly.ImageRuntimeVersion.Substring(1),
                    MimeType = "text/x-csharp",
                    FileExtension = ".cs",
                    PygmentsLexer = "c#"
                }
            };
            var header = new Header
            {
                Username = Constants.USERNAME,
                Session = "test session",
                MessageId = Guid.NewGuid().ToString(),
                MessageType = MessageTypeValues.KernelInfoReply,
                Date = DateTime.MinValue.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                Version = "5.3"
            };
            var replyMessage = new Message
            {
                Header = header,
                Content = kernelInfoReply
            };
            sender.Send(replyMessage);

            var encoded = socket.GetEncodedMessage();
            this.Assent(encoded, _configuration);
        }
    }
    public class JupyterAdapterTests
    {
        [Fact]
        public void do_it()
        {
             // dispatch a jupyter execute request

             var executeRequest = new ExecuteRequest
             {
                 Code = "Console.WriteLine(12);"
             };

            throw new NotImplementedException();
        }
    }
}
