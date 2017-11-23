using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WorkspaceServer.Servers.Scripting
{
    public class RedirectConsoleOutput : IDisposable
    {
        private TextWriter originalWriter;
        private StringWriter writer = new StringWriter();

        private const int NOT_DISPOSED = 0;
        private const int DISPOSED = 1;

        private int alreadyDisposed = NOT_DISPOSED;

        private static SemaphoreSlim consoleLock = new SemaphoreSlim(1, 1);


        private RedirectConsoleOutput()
        {
        }

        public static async Task<RedirectConsoleOutput> Acquire()
        {
            var redirector = new RedirectConsoleOutput();
            await consoleLock.WaitAsync();

            try
            {
                redirector.originalWriter = Console.Out;
                Console.SetOut(redirector.writer);
            }
            catch
            {
                consoleLock.Release();
                throw;
            }

            return redirector;
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref alreadyDisposed, DISPOSED, NOT_DISPOSED) == NOT_DISPOSED)
            {
                // This must only happen once.
                consoleLock.Release();
            }

            Console.SetOut(originalWriter);
        }

        public override string ToString() => writer.ToString().Trim();

        public void Clear() => writer.GetStringBuilder().Clear();

        public bool IsEmpty() => writer.ToString().Length == 0;
    }
}
