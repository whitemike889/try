using System;
using MLS.Agent;
using MLS.Agent.Tools;
using OmniSharp.Client;
using WorkspaceServer.Servers.Dotnet;
using Xunit.Abstractions;

namespace Pocket
{
    internal partial class LogEvents
    {
        public static IDisposable SubscribeToPocketLogger(this ITestOutputHelper output) =>
            Subscribe(
                e => output.WriteLine(e.ToLogString()),
                new[]
                {
                    typeof(Startup).Assembly,
                    typeof(CommandLine).Assembly,
                    typeof(OmniSharpServer).Assembly,
                    typeof(OmniSharpMessage).Assembly,
                });
    }
}
