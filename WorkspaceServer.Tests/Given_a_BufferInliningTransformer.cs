using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Transformations;
using Xunit;

namespace WorkspaceServer.Tests
{
    public class Given_a_BufferInliningTransformer
    {
        [Fact]
        public void It_extracts_viewPorts_when_files_declare_region()
        {
            var ws = new Workspace(files: new[]
            {
                new Workspace.File("Program.cs", Properties.Resources.SingleRegionCode)
            });
            var processor = new BufferInliningTransformer();
            var viewPorts = processor.ExtractViewPorts(ws);
            viewPorts.Should().NotBeEmpty();
            viewPorts.Keys.ShouldAllBeEquivalentTo(new[] { "alpha" });
        }

        [Fact]
        public void ViewPort_ids_must_be_uinique_within_a_file()
        {
            var ws = new Workspace(files: new[]
            {
                new Workspace.File("Program.cs", Properties.Resources.ConflictingRegionCode)
            });
            var processor = new BufferInliningTransformer();
            Action extraction = () => processor.ExtractViewPorts(ws);
            extraction.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void ViewPort_ids_must_be_uinique_inside_the_workspace()
        {
            var ws = new Workspace(files: new[]
            {
                new Workspace.File("ProgramA.cs", Properties.Resources.SingleRegionCode),
                new Workspace.File("ProgramB.cs", Properties.Resources.SingleRegionCode)
            });
            var processor = new BufferInliningTransformer();
            Action extraction = () => processor.ExtractViewPorts(ws);
            extraction.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void ViewPort_extraction_fails_with_null_workspace()
        {
            var processor = new BufferInliningTransformer();
            Action extraction = () => processor.ExtractViewPorts(null);
            extraction.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Processing_fails_with_null_workspace()
        {
            var processor = new BufferInliningTransformer();
            Func<Task> extraction = () => processor.ProcessAsync(null);
            extraction.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public async Task Processed_workspace_files_are_modified_inlining_buffers()
        {
            var ws = new Workspace(
                files: new[]
                {
                    new Workspace.File("Program.cs", Properties.Resources.SingleRegionCode)
                },
                buffers: new[]
                {
                    new Workspace.Buffer("alpha", "var newValue = 1000;", 0)
                });
            var processor = new BufferInliningTransformer();

            var processed = await processor.ProcessAsync(ws);
            processed.Should().NotBeNull();
            processed.SourceFiles.Should().NotBeEmpty();
            var newCode = processed.SourceFiles.ElementAt(0).Text.ToString();

            newCode.Should().NotBe(ws.SourceFiles.ElementAt(0).Text.ToString());
            newCode.Should().Contain("var newValue = 1000;");

            processed.Buffers.Count.Should().Be(ws.Buffers.Count);
            processed.Buffers.ElementAt(0).Position.Should().BeGreaterThan(ws.Buffers.ElementAt(0).Position);

        }

        [Fact]
        public async Task Processed_workspace_with_single_buffer_with_empty_id_generates_a_program_file()
        {
            var ws = new Workspace(
                buffers: new[]
                {
                    new Workspace.Buffer("", Properties.Resources.SingleRegionCode, 0)
                });
            var processor = new BufferInliningTransformer();

            var processed = await processor.ProcessAsync(ws);
            processed.Should().NotBeNull();
            processed.SourceFiles.Should().NotBeEmpty();
            var newCode = processed.SourceFiles.ElementAt(0).Text.ToString();
            newCode.Should().Contain(Properties.Resources.SingleRegionCode);

        }
    }
}