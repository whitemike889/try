using FluentAssertions;
using MLS.Protocol.Execution;
using MLS.Protocol.Generators;
using Xunit;

namespace MLS.Protocol.Tests
{
    public class BufferCreationTests
    {
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
