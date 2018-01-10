using System;
using System.Diagnostics;
using FluentAssertions;

namespace WorkspaceServer.Tests
{
    internal static class Default
    {
        public static TimeSpan Timeout() =>
            Debugger.IsAttached
                ? 10.Minutes()
                : 20.Seconds();
    }
}