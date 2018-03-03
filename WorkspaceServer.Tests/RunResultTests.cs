using FluentAssertions;
using Pocket;
using WorkspaceServer.Models.Execution;
using Xunit;

namespace WorkspaceServer.Tests
{
    public class RunResultTests
    {
        [Fact]
        public void Disposable_RunResult_features_are_disposed_when_RunResult_is_disposed()
        {
            var wasDisposed = false;

            var result = new RunResult(true);

            result.AddFeature(Disposable.Create(() => wasDisposed = true));

            result.Dispose();

            wasDisposed.Should().BeTrue();
        }
    }
}
