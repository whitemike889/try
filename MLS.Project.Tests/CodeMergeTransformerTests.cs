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
        public void Processing_fails_with_null_workspace()
        {
            var processor = new CodeMergeTransformer();
            Func<Task> extraction = () => processor.TransformAsync(null);
            extraction.Should().Throw<ArgumentNullException>();
        }
    }
}