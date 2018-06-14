using System;
using FluentAssertions;
using OmniSharp.Client;
using WorkspaceServer.Models;
using Xunit;

namespace WorkspaceServer.Tests
{
    public class DiagnosticTests
    {
        [Fact]
        public void ToString_formats_location_and_error_code_and_message()
        {
            var diagnostic = new Diagnostic(
                id: "CS0103",
                message: "The name 'banana' does not exist in the current context",
                location: new Location(
                    mappedLineSpan:
                    new FileLinePositionSpan(
                        startLinePosition:
                        new LinePosition(2, 19, true))));

            diagnostic.ToString()
                      .Should()
                      .Be("(2,19): error CS0103: The name 'banana' does not exist in the current context");
        }
    }
}
