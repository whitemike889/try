using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Processors;
using Xunit;

namespace WorkspaceServer.Tests
{
    public class BufferInliningProcessorTests
    {
        [Fact]
        public void Extracts_viewPorts_when_files_declare_region()
        {
            var ws = new WorkspaceRunRequest(files: new[]
            {
                new WorkspaceRunRequest.File("Program.cs", Properties.Resources.ConsoleProgramSingleRegion)
            });
            var processor = new BufferInliningProcessor();
            var viewPorts = processor.ExtractViewPorts(ws);
            viewPorts.Should().NotBeEmpty();
            viewPorts.Keys.ShouldAllBeEquivalentTo(new[] { "alpha" });
        }

        [Fact]
        public void ViewPort_ids_must_be_uinique_within_a_file()
        {
            var ws = new WorkspaceRunRequest(files: new[]
            {
                new WorkspaceRunRequest.File("Program.cs", Properties.Resources.ConsoleProgramCollidingRegions)
            });
            var processor = new BufferInliningProcessor();
            Action extraction = () => processor.ExtractViewPorts(ws);
            extraction.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void ViewPort_ids_must_be_uinique_inside_the_workspace()
        {
            var ws = new WorkspaceRunRequest(files: new[]
            {
                new WorkspaceRunRequest.File("ProgramA.cs", Properties.Resources.ConsoleProgramSingleRegion),
                new WorkspaceRunRequest.File("ProgramB.cs", Properties.Resources.ConsoleProgramSingleRegion)
            });
            var processor = new BufferInliningProcessor();
            Action extraction = () => processor.ExtractViewPorts(ws);
            extraction.ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void ViewPort_extraction_fails_with_null_workspace()
        {
            var processor = new BufferInliningProcessor();
            Action extraction = () => processor.ExtractViewPorts(null);
            extraction.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Processing_fails_with_null_workspace()
        {
            var processor = new BufferInliningProcessor();
            Func<Task> extraction = () => processor.ProcessAsync(null);
            extraction.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public async Task Processed_workspace_files_are_modified_inlining_buffers()
        {
            var ws = new WorkspaceRunRequest(
                files: new[]
                {
                    new WorkspaceRunRequest.File("Program.cs", Properties.Resources.ConsoleProgramSingleRegion)
                },
                buffers: new[]
                {
                    new WorkspaceRunRequest.Buffer("alpha", "var newValue = 1000;", 0)
                });
            var processor = new BufferInliningProcessor();

            var processed = await processor.ProcessAsync(ws);
            processed.Should().NotBeNull();
            processed.SourceFiles.Should().NotBeEmpty();
            processed.SourceFiles.ElementAt(0).Text.ToString().Should()
                .NotBe(ws.SourceFiles.ElementAt(0).Text.ToString());
            processed.SourceFiles.ElementAt(0).Text.ToString().Should().Contain("var newValue = 1000;");

        }

        [Fact]
        public async Task Processed_workspace_with_single_buffer_with_empty_id_generates_a_program_file()
        {
            var ws = new WorkspaceRunRequest(
                buffers: new[]
                {
                    new WorkspaceRunRequest.Buffer("", Properties.Resources.ConsoleProgramSingleRegion, 0)
                });
            var processor = new BufferInliningProcessor();

            var processed = await processor.ProcessAsync(ws);
            processed.Should().NotBeNull();
            processed.SourceFiles.Should().NotBeEmpty();
            processed.SourceFiles.ElementAt(0).Text.ToString().Should().Contain(Properties.Resources.ConsoleProgramSingleRegion);

        }
    }
}