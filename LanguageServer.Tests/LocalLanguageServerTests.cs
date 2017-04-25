using System;
using System.IO;
using Xunit.Abstractions;

namespace LanguageServer.Tests
{
    public class LocalLanguageServerTests : LanguageServerTests
    {
        protected override ILanguageServer GetLanguageServer()
        {
            var directoryName = Guid.NewGuid().ToString();

            var directory = new DirectoryInfo(directoryName);

            return new LocalLanguageServer(directory);
        }

        public LocalLanguageServerTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}