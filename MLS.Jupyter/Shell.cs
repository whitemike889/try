using System;
using System.Threading;
using MLS.Jupyter.Protocol;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pocket;
using static Pocket.Logger<MLS.Jupyter.HearthBeatHandler>;

namespace MLS.Jupyter
{
    public class Shell : IDisposable
    {
        private readonly RouterSocket _server;
        private readonly PublisherSocket _ioPubSocket;
        private readonly ManualResetEventSlim _stopEvent;
        private readonly string _shellAddress;
        private readonly string _ioPubAddress;
        private readonly SignatureValidator _signatureValidator;
        private Thread _thread;
        private bool _disposed;

        public Shell(ConnectionInformation connectionInformation)
        {
            if (connectionInformation == null)
                throw new ArgumentNullException(nameof(connectionInformation));

            _shellAddress = $"{connectionInformation.Transport}://{connectionInformation.IP}:{connectionInformation.ShellPort}";
            _ioPubAddress = $"{connectionInformation.Transport}://{connectionInformation.IP}:{connectionInformation.IOPubPort}";
            var signatureAlgorithm = connectionInformation.SignatureScheme.Replace("-", string.Empty).ToUpperInvariant();
            _signatureValidator = new SignatureValidator(connectionInformation.Key, signatureAlgorithm);
            _server = new RouterSocket();
            _ioPubSocket = new PublisherSocket();
            _stopEvent = new ManualResetEventSlim();
        }

        public void Start()
        {
            ThrowIfDisposed();
            _stopEvent.Reset();
            if (_thread == null)
            {
                _thread = new Thread(StartServerLoop);
                _thread.Start();
            }

        }

        public void Stop()
        {
            _stopEvent.Set();
        }

        private void StartServerLoop(object state)
        {
            _server.Bind(_shellAddress);
            _ioPubSocket.Bind(_ioPubAddress);

            while (!_stopEvent.Wait(0))
            {
                var message = _server.GetMessage();

                var messageType = message.Header.MessageType;
                switch (messageType)
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

        private void HandleKernelInfoRequest(Message message)
        {
            SendStatus(message, _ioPubSocket, StatusValues.Busy);

            var kernelInfoReply = new KernelInfoReply()
            {
                ProtocolVersion = "5.3",
                Implementation = "dotnet",
                ImplementationVersion = "0.0.3",
                LanguageInfo = new JObject()
                {
                    { "name",  "dotnet" },
                    { "version", typeof(string).Assembly.ImageRuntimeVersion.Substring(1) },
                    { "mimetype", "text/x-csharp" },
                    { "file_extension", ".cs"},
                    { "pygments_lexer", "c#" }
                }
            };

            var replyMessage = new Message()
            {
                Identifiers = message.Identifiers,
                Signature = message.Signature,
                ParentHeader = message.Header,
                Header = CreateHeader(MessageTypeValues.KernelInfoReply, message.Header.Session),
                Content = JObject.FromObject(kernelInfoReply)
            };
         
            Send(replyMessage, _server);

            // 3: Send IDLE status message to IOPub
           SendStatus(message, _ioPubSocket, StatusValues.Idle);
        }
     

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(HearthBeatHandler));
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected void Dispose(bool dispose)
        {
            if (!_disposed && dispose)
            {
                _disposed = true;
                _server?.Dispose();
            }
        }

        public bool Send(Message message, NetMQSocket socket)
        {
            string hmac = this._signatureValidator.CreateSignature(message);

            foreach (var ident in message.Identifiers)
            {
                socket.TrySendFrame(ident, true);
            }

            Send(Constants.DELIMITER, socket);
            Send(hmac, socket);
            Send(JsonConvert.ToString(message.Header), socket);
            Send(JsonConvert.ToString(message.ParentHeader), socket);
            Send(JsonConvert.ToString(message.MetaData), socket);
            Send(JsonConvert.ToString(message.Content), socket, false);

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

            var message = new Message()
            {
                ParentHeader = parentHeader,
                Header = CreateHeader(messageType, session),
                Content = content
            };

            return message;
        }

    }
}