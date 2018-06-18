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
                new Workspace.File("Program.cs", CodeManipulation.EnforceLF(CodeSamples.SourceCodeProvider.ConsoleProgramSingleRegion))
            });
            var processor = new BufferInliningTransformer();
            var viewPorts = processor.ExtractViewPorts(ws);
            viewPorts.Should().NotBeEmpty();
            viewPorts.Keys.Should().BeEquivalentTo("Program.cs@alpha");
        }

        [Fact]
        public void ViewPort_ids_must_be_uinique_within_a_file()
        {
            var ws = new Workspace(files: new[]
            {
                new Workspace.File("Program.cs", CodeManipulation.EnforceLF(CodeSamples.SourceCodeProvider.ConsoleProgramCollidingRegions))
            });
            var processor = new BufferInliningTransformer();
            Action extraction = () => processor.ExtractViewPorts(ws);
            extraction.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ViewPort_ids_must_be_uinique_inside_the_workspace()
        {
            var ws = new Workspace(files: new[]
            {
                new Workspace.File("ProgramA.cs", CodeManipulation.EnforceLF(CodeSamples.SourceCodeProvider.ConsoleProgramSingleRegion)),
                new Workspace.File("ProgramB.cs", CodeManipulation.EnforceLF(CodeSamples.SourceCodeProvider.ConsoleProgramSingleRegion))
            });
            var processor = new BufferInliningTransformer();
            Action extraction = () => processor.ExtractViewPorts(ws);
            extraction.Should().NotThrow<ArgumentException>();
        }

        [Fact]
        public void ViewPort_extraction_fails_with_null_workspace()
        {
            var processor = new BufferInliningTransformer();
            Action extraction = () => processor.ExtractViewPorts(null);
            extraction.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Processing_fails_with_null_workspace()
        {
            var processor = new BufferInliningTransformer();
            Func<Task> extraction = () => processor.TransformAsync(null);
            extraction.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task Processed_workspace_files_are_modified_inlining_buffers()
        {
            var ws = new Workspace(
                files: new[]
                {
                    new Workspace.File("Program.cs", CodeManipulation.EnforceLF(CodeSamples.SourceCodeProvider.ConsoleProgramSingleRegion))
                },
                buffers: new[]
                {
                    new Workspace.Buffer("Program.cs@alpha", CodeManipulation.EnforceLF("var newValue = 1000;"), 0)
                });
            var processor = new BufferInliningTransformer();

            var processed = await processor.TransformAsync(ws);
            processed.Should().NotBeNull();
            processed.Files.Should().NotBeEmpty();
            var newCode = processed.Files.ElementAt(0).Text;

            newCode.Should().NotBe(ws.Files.ElementAt(0).Text);
            newCode.Should().Contain("var newValue = 1000;");

            processed.Buffers.Count.Should().Be(ws.Buffers.Count);
            processed.Buffers.ElementAt(0).Position.Should().BeGreaterThan(ws.Buffers.ElementAt(0).Position);

        }

        [Fact]
        public async Task Processed_workspace_files_are_replaced_by_buffer_when_id_is_just_file_name()
        {
            var ws = new Workspace(
                files: new[]
                {
                    new Workspace.File("Program.cs", CodeManipulation.EnforceLF(CodeSamples.SourceCodeProvider.ConsoleProgramSingleRegion))
                },
                buffers: new[]
                {
                    new Workspace.Buffer("Program.cs", CodeManipulation.EnforceLF("var newValue = 1000;"), 0)
                });
            var processor = new BufferInliningTransformer();

            var processed = await processor.TransformAsync(ws);
            processed.Should().NotBeNull();
            processed.Files.Should().NotBeEmpty();
            var newCode = processed.Files.ElementAt(0).Text;

            newCode.Should().NotBe(ws.Files.ElementAt(0).Text);
            newCode.Should().Be("var newValue = 1000;");

            processed.Buffers.Count.Should().Be(ws.Buffers.Count);
            processed.Buffers.ElementAt(0).Position.Should().Be(0);
        }

        [Fact]
        public async Task Processed_workspace_with_single_buffer_with_empty_id_generates_a_program_file()
        {
            var ws = new Workspace(
                buffers: new[]
                {
                    new Workspace.Buffer("", CodeManipulation.EnforceLF(CodeSamples.SourceCodeProvider.ConsoleProgramSingleRegion), 0)
                });
            var processor = new BufferInliningTransformer();

            var processed = await processor.TransformAsync(ws);
            processed.Should().NotBeNull();
            processed.Files.Should().NotBeEmpty();
            var newCode = processed.Files.ElementAt(0).Text;
            newCode.Should().Contain(CodeManipulation.EnforceLF(CodeSamples.SourceCodeProvider.ConsoleProgramSingleRegion));

        }
    }
}