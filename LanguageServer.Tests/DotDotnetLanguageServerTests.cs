using Xunit.Abstractions;

namespace LanguageServer.Tests
{
    public class DotDotnetLanguageServerTests : LanguageServerTests
    {
        protected override ILanguageServer GetLanguageServer()
        {
            return new DotDotnetLanguageServer();
        }

        public DotDotnetLanguageServerTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}