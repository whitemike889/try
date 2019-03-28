using System;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using MLS.Jupyter.Protocol;
using NetMQ.Sockets;
using Pocket;
using static Pocket.Logger<MLS.Jupyter.Shell>;

namespace MLS.Jupyter
{
    public class Shell : IHostedService, IObservable<JupyterRequestContext>
    {
        private readonly RouterSocket _server;
        private readonly PublisherSocket _ioPubSocket;
        private readonly string _shellAddress;
        private readonly string _ioPubAddress;
        private readonly SignatureValidator _signatureValidator;
        private readonly CompositeDisposable _disposables;
        private readonly MessageBuilder _messageBuilder;
        private readonly MessageSender _serverMessageSender;
        private readonly MessageSender _ioPubSender;
        private readonly Subject<JupyterRequestContext> _channel = new Subject<JupyterRequestContext>();
        private readonly IObservable<JupyterRequestContext> _subscriptionChannel;

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

            _serverMessageSender = new MessageSender(_server, _signatureValidator);
            _ioPubSender = new MessageSender(_ioPubSocket, _signatureValidator);

            _disposables = new CompositeDisposable
                           {
                               _server,
                               _ioPubSocket
                           };
            _messageBuilder = new MessageBuilder();
            _subscriptionChannel = _channel;
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
                            break;

                        case MessageTypeValues.KernelShutdownRequest:
                            break;
                        default:
                            Log.Info($"Forward request context {message.Header.MessageType}");
                            var context = new JupyterRequestContext(
                                _messageBuilder,
                                _serverMessageSender,
                                _ioPubSender,
                                message,
                                new RequestHandlerStatus(message.Header, _serverMessageSender));
                            _channel.OnNext(context);
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
            var status = new RequestHandlerStatus(message.Header, new MessageSender(_ioPubSocket, _signatureValidator));
            status.SetAsBusy();

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

            var replyMessage = new Message
            {
                Identifiers = message.Identifiers,
                Signature = message.Signature,
                ParentHeader = message.Header,
                Header = _messageBuilder.CreateHeader(MessageTypeValues.KernelInfoReply, message.Header.Session),
                Content = kernelInfoReply
            };
            _serverMessageSender.Send(replyMessage);

            // 3: Send IDLE status message to IOPub
            status.SetAsIdle();
        }

        public IDisposable Subscribe(IObserver<JupyterRequestContext> observer) => _subscriptionChannel.Subscribe(observer);
    }
}