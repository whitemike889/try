using System;
using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;
using Pocket;
using Recipes;
using WorkspaceServer.Models.Completion;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Servers.Scripting;
using Xunit;
using Xunit.Abstractions;
using static Pocket.Logger<WorkspaceServer.Tests.ScriptingWorkspaceServerTests>;

namespace WorkspaceServer.Tests
{
    public class RunResultExpectationTests
    {
        [Fact]
        public void RunResultHasOutput()
        {
            nameof(RunResult.Output).Should().Be("Output");
        }

        [Fact]
        public void RunResultHasSucceeded()
        {
            nameof(RunResult.Succeeded).Should().Be("Succeeded");
        }

        [Fact]
        public void RunResultHasException()
        {
            nameof(RunResult.Exception).Should().Be("Exception");
        }

        [Fact]
        public void RunResultHasVariables()
        {
            nameof(RunResult.Variables).Should().Be("Variables");
        }

        [Fact]
        public void RunResultHasReturnValue()
        {
            nameof(RunResult.ReturnValue).Should().Be("ReturnValue");
        }
    }
}
