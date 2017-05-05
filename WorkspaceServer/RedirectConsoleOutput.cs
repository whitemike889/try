using System;
using System.IO;

namespace WorkspaceServer
{
    public class RedirectConsoleOutput : IDisposable
    {
        private readonly TextWriter originalWriter;
        private readonly StringWriter writer = new StringWriter();

        public RedirectConsoleOutput()
        {
            originalWriter = Console.Out;
            Console.SetOut(writer);
        }

        public void Dispose() => Console.SetOut(originalWriter);

        public override string ToString() => writer.ToString().Trim();

        public void Clear() => writer.GetStringBuilder().Clear();
    }
}