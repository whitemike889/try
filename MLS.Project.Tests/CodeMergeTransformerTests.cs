using System;
using System.Threading.Tasks;
using FluentAssertions;
using MLS.Project.Transformations;
using Xunit;

namespace MLS.Project.Tests
{
    public class CodeMergeTransformerTests
    {
        [Fact]
        public void When_workspace_is_null_then_the_transformer_throw_exceptio()
        {
            var processor = new CodeMergeTransformer();
            Func<Task> extraction = () => processor.TransformAsync(null);
            extraction.Should().Throw<ArgumentNullException>();
        }
    }
}