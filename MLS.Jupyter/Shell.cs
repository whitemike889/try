using System;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using MLS.Jupyter.Protocol;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json.Linq;
using Pocket;
using Recipes;
using static Pocket.Logger<MLS.Jupyter.Heartbeat>;

namespace MLS.Jupyter
{
    public class Shell : IHostedService
    {
        private readonly RouterSocket _server;
        private readonly PublisherSocket _ioPubSocket;
        private readonly string _shellAddress;
        private readonly string _ioPubAddress;
        private readonly SignatureValidator _signatureValidator;
        private readonly CompositeDisposable _disposables;

        public Shell(ConnectionInformation connectionInformation)
        {
            if (connectionInformation == null)
            {
                throw new ArgumentNullException(nameof(connectionInformation));
            }

            _shellAddress = $"{connectionInformation.Transport}://{connectionInformation.IP}:{connectionInformation.ShellPort}";
            _ioPubAddress = $"{connectionInformation.Transport}://{connectionInformation.IP}:{connectionInformation.IOPubPort}";
            var signatureAlgorithm = connectionInformation.SignatureScheme.Replace("-", string.Empty).ToUpperInvariant();
            _signatureValidator = new SignatureValidator(connectionInformation.Key, signatureAlgorithm);
            _server = new RouterSocket();
            _ioPubSocket = new PublisherSocket();

            _disposables = new CompositeDisposable
                           {
                               _server, 
                               _ioPubSocket
                           };
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _server.Bind(_shellAddress);
            _ioPubSocket.Bind(_ioPubAddress);

            using (Log.OnEnterAndExit())
            while (!cancellationToken.IsCancellationRequested)
            {
                var message = _server.GetMessage();

                Log.Info("{message}", message);

                switch (message.Header.MessageType)
                {
                    case MessageTypeValues.KernelInfoRequest:
                        HandleKernelInfoRequest(message);
                        Log.Info("KernelInfoRequest");
                        break;

                    case MessageTypeValues.KernelShutdownRequest:
                        Log.Info("KernelShutdownRequest");
                        break;
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _disposables.Dispose();
            return Task.CompletedTask;
        }

        private void HandleKernelInfoRequest(Message message)
        {
            SendStatus(message, _ioPubSocket, StatusValues.Busy);

            var kernelInfoReply = new KernelInfoReply
                                  {
                                      ProtocolVersion = "5.3",
                                      Implementation = ".NET",
                                      ImplementationVersion = "0.0.3",
                                      LanguageInfo = new LanguageInfo
                                                     {
                                                         Name = "dotnet",
                                                         Version = typeof(string).Assembly.ImageRuntimeVersion.Substring(1),
                                                         MimeType = "text/x-csharp",
                                                         FileExtension = ".cs",
                                                         PygmentsLexer = "c#"
                                                     }
                                  };

            var replyMessage = new Message
                               {
                                   Identifiers = message.Identifiers,
                                   Signature = message.Signature,
                                   ParentHeader = message.Header,
                                   Header = CreateHeader(MessageTypeValues.KernelInfoReply, message.Header.Session),
                                   Content = kernelInfoReply
                               };

            Send(replyMessage, _server);

            // 3: Send IDLE status message to IOPub
            SendStatus(message, _ioPubSocket, StatusValues.Idle);
        }

        public bool Send(Message message, NetMQSocket socket)
        {
            string hmac = _signatureValidator.CreateSignature(message);

            foreach (var ident in message.Identifiers)
            {
                socket.TrySendFrame(ident, true);
            }

            Send(Constants.DELIMITER, socket);
            Send(hmac, socket);
            Send(message.Header.ToJson(), socket);
            Send(message.ParentHeader.ToJson(), socket);
            Send(message.MetaData.ToJson(), socket);
            Send(message.Content.ToJson(), socket, false);

            return true;
        }

        private static void Send(string message, IOutgoingSocket socket, bool sendMore = true)
        {
            socket.SendFrame(message, sendMore);
        }

        public bool SendStatus(Message message, PublisherSocket ioPub, string status)
        {
            var content = new Status
                          {
                              ExecutionState = status
                          };

            var ioPubMessage = CreateMessage(MessageTypeValues.Status, JObject.FromObject(content), message.Header);

            return Send(ioPubMessage, ioPub);
        }

        public static Header CreateHeader(string messageType, string session)
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

        public static Message CreateMessage(string messageType, JObject content, Header parentHeader)
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