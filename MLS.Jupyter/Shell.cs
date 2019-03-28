using System;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Clockwise;
using Microsoft.Extensions.Hosting;
using MLS.Jupyter.Protocol;
using NetMQ.Sockets;
using Pocket;
using Recipes;
using static Pocket.Logger<MLS.Jupyter.Shell>;

namespace MLS.Jupyter
{
    public class Shell : IHostedService
    {
        private readonly ICommandScheduler<JupyterRequestContext> _scheduler;
        private readonly RouterSocket _server;
        private readonly PublisherSocket _ioPubSocket;
        private readonly string _shellAddress;
        private readonly string _ioPubAddress;
        private readonly SignatureValidator _signatureValidator;
        private readonly CompositeDisposable _disposables;
        private readonly MessageBuilder _messageBuilder;
        private readonly MessageSender _serverMessageSender;
        private readonly MessageSender _ioPubSender;

        public Shell(
            ICommandScheduler<JupyterRequestContext> scheduler,
            ConnectionInformation connectionInformation)
        {
            if (connectionInformation == null)
            {
                throw new ArgumentNullException(nameof(connectionInformation));
            }

            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));

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
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _server.Bind(_shellAddress);
            _ioPubSocket.Bind(_ioPubAddress);

            using (var activity = Log.OnEnterAndExit())
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var message = _server.GetMessage();

                    activity.Info("Received: {message}", message.ToJson());

                    var status = new RequestHandlerStatus(message.Header, new MessageSender(_ioPubSocket, _signatureValidator));
                    status.SetAsBusy();

                    switch (message.Header.MessageType)
                    {
                        case MessageTypeValues.KernelInfoRequest:
                            HandleKernelInfoRequest(message);
                            break;

                        case MessageTypeValues.KernelShutdownRequest:
                            break;

                        default:
                            var context = new JupyterRequestContext(
                                _messageBuilder,
                                _serverMessageSender,
                                _ioPubSender,
                                message,
                                new RequestHandlerStatus(message.Header, _serverMessageSender));

                            await _scheduler.Schedule(context);

                            break;
                    }

                    status.SetAsIdle();
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
        }
    }
}