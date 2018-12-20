using System;
using System.CommandLine;
using System.CommandLine.Rendering;
using System.IO;

namespace MLS.Agent.Tests
{
    public class TestConsole : IConsole
    {
        public TestConsole()
        {
            Error = StandardStreamWriter.Create(new StringWriter());
            Out = StandardStreamWriter.Create(new StringWriter());
        }

        public IStandardStreamWriter Error { get; }

        public void SetOut(TextWriter writer)
        {
            Out = StandardStreamWriter.Create(writer);
            IsOutputRedirected = true;
        }

        public Region GetRegion() => new Region(120, 80, 0, 0);

        public IStandardStreamWriter Out { get; private set; }

        public bool IsOutputRedirected { get; private set; }

        public bool IsErrorRedirected { get; private set; }

        public void Dispose()
        {
        }
    }
}