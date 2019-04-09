using System.Linq;
using FluentAssertions;
using Microsoft.DotNet.Try.Project.Generators;
using Microsoft.DotNet.Try.Protocol.Execution;
using Xunit;

namespace Microsoft.DotNet.Try.Project.Tests
{
    public class BufferCreationTests
    {
        [Fact]
        public void can_create_buffers_from_file_with_regions()
        {
            var file = FileGenerator.Create("Program.cs", MLS.TestSupport.SourceCodeProvider.ConsoleProgramMultipleRegions);

            var buffers = BufferGenerator.CreateFromFile(file).ToList();

            buffers.Should().NotBeNullOrEmpty();
            buffers.Count.Should().Be(2);
            buffers.Should().Contain(b => b.Id == "Program.cs@alpha");
            buffers.Should().Contain(b => b.Id == "Program.cs@beta");
        }

        [Fact]
        public void can_create_buffers_from_file_without_regions()
        {
            var file = FileGenerator.Create("Program.cs", MLS.TestSupport.SourceCodeProvider.ConsoleProgramNoRegion);

            var buffers = BufferGenerator.CreateFromFile(file).ToList();

            buffers.Should().NotBeNullOrEmpty();
            buffers.Count.Should().Be(1);
            buffers.Should().Contain(b => b.Id == "Program.cs");
        }

        [Fact]
        public void can_create_buffer_from_code_and_bufferId()
        {
            var buffer = BufferGenerator.CreateBuffer("Console.WriteLine(12);", "program.cs");
            buffer.Should().NotBeNull();
            buffer.Id.Should().Be(new BufferId("program.cs"));
            buffer.Content.Should().Be("Console.WriteLine(12);");
            buffer.AbsolutePosition.Should().Be(0);
        }

        [Fact]
        public void can_create_buffer_with_bufferId_and_region()
        {
            var buffer = BufferGenerator.CreateBuffer("Console.WriteLine(12);", "program.cs@region1");
            buffer.Should().NotBeNull();
            buffer.Id.Should().Be(new BufferId("program.cs", "region1"));
            buffer.Content.Should().Be("Console.WriteLine(12);");
            buffer.AbsolutePosition.Should().Be(0);
        }

        [Fact]
        public void can_create_buffer_with_markup()
        {
            var buffer = BufferGenerator.CreateBuffer("Console.WriteLine($$);", "program.cs@region1");
            buffer.Should().NotBeNull();
            buffer.Id.Should().Be(new BufferId("program.cs", "region1"));
            buffer.Content.Should().Be("Console.WriteLine();");
            buffer.AbsolutePosition.Should().Be(18);
        }
    }
}
