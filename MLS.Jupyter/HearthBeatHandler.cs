using System;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;
using Pocket;
using static Pocket.Logger<MLS.Jupyter.HearthBeatHandler>;

namespace MLS.Jupyter
{
    public class HearthBeatHandler : IDisposable
    {

        private readonly string _address;
        private readonly ResponseSocket _server;
        private readonly ManualResetEventSlim _stopEvent;
        private Thread _thread;
        private bool _disposed;

        public HearthBeatHandler(ConnectionInformation connectionInformation)
        {
            if (connectionInformation == null)
            {
                throw new ArgumentNullException(nameof(connectionInformation));
            }

            _address = $"{connectionInformation.Transport}://{connectionInformation.IP}:{connectionInformation.HBPort}";

            Log.Info($"using address {nameof(_address)}", _address);
            _server = new ResponseSocket();
            _stopEvent = new ManualResetEventSlim();
        }

        public void Start()
        {
            ThrowIfDisposed();
            _stopEvent.Reset();
            if (_thread == null)
            {
                _thread = new Thread(StartServerLoop);
                _thread.Name = "Kernel HeartBeat server";
                _thread.Start();
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(HearthBeatHandler));
            }
        }

        private void StartServerLoop(object state)
        {
            _server.Bind(_address);
            using (Log.OnEnterAndExit())
            {
                while (!_stopEvent.Wait(0))
                {
                    var data = _server.ReceiveFrameBytes();

                    // Echoing back whatever was received
                    _server.TrySendFrame(data);
                }
            }
            _thread = null;
        }

        public void Stop()
        {
            ThrowIfDisposed();
            _stopEvent.Set();
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
    }
}
