using System;
using System.Collections.Generic;
using System.IO;

namespace WorkspaceServer.Servers.Scripting
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

        public bool IsEmpty() => writer.ToString().Length == 0;

        public void WriteLines(IEnumerable<string> lines)
        {
            foreach (var line in lines)
            {
                writer.WriteLine(line);
            }
        }
    }
}
