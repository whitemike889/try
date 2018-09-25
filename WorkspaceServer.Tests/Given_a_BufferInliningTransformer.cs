using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MLS.Protocol.Execution;
using MLS.Protocol.Transformations;
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
                new Workspace.File("Program.cs", CodeSamples.SourceCodeProvider.ConsoleProgramSingleRegion)
            });
            var processor = new BufferInliningTransformer();
            var viewPorts = processor.ExtractViewPorts(ws);
            viewPorts.Should().NotBeEmpty();
            viewPorts.Select(p => p.BufferId.ToString()).Should().BeEquivalentTo("Program.cs@alpha");
        }

        [Fact]
        public void ViewPort_ids_must_be_uinique_within_a_file()
        {
            var ws = new Workspace(files: new[]
            {
                new Workspace.File("Program.cs", CodeSamples.SourceCodeProvider.ConsoleProgramCollidingRegions)
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
                new Workspace.File("ProgramA.cs", CodeSamples.SourceCodeProvider.ConsoleProgramSingleRegion),
                new Workspace.File("ProgramB.cs", CodeSamples.SourceCodeProvider.ConsoleProgramSingleRegion)
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
            var original = new Workspace(
                files: new[]
                {
                    new Workspace.File("Program.cs", CodeSamples.SourceCodeProvider.ConsoleProgramSingleRegion)
                },
                buffers: new[]
                {
                    new Workspace.Buffer("Program.cs@alpha", "var newValue = 1000;".EnforceLF())
                });
            var processor = new BufferInliningTransformer();

            var processed = await processor.TransformAsync(original);
            processed.Should().NotBeNull();
            processed.Files.Should().NotBeEmpty();
            var newCode = processed.Files.ElementAt(0).Text;

            newCode.Should().NotBe(original.Files.ElementAt(0).Text);
            newCode.Should().Contain("var newValue = 1000;");

            original.Buffers.ElementAt(0).Position.Should().Be(0);
            processed.Buffers.Length.Should().Be(original.Buffers.Length);
            processed.Buffers.ElementAt(0).Position.Should().Be(original.Buffers.ElementAt(0).Position);
            processed.Buffers.ElementAt(0).AbsolutePosition.Should().Be(168);

        }

        [Fact]
        public async Task Processed_workspace_files_are_replaced_by_buffer_when_id_is_just_file_name()
        {
            var ws = new Workspace(
                files: new[]
                {
                    new Workspace.File("Program.cs", CodeSamples.SourceCodeProvider.ConsoleProgramSingleRegion)
                },
                buffers: new[]
                {
                    new Workspace.Buffer("Program.cs", "var newValue = 1000;", 0)
                });
            var processor = new BufferInliningTransformer();

            var processed = await processor.TransformAsync(ws);
            processed.Should().NotBeNull();
            processed.Files.Should().NotBeEmpty();
            var newCode = processed.Files.ElementAt(0).Text;

            newCode.Should().NotBe(ws.Files.ElementAt(0).Text);
            newCode.Should().Be("var newValue = 1000;");

            processed.Buffers.Length.Should().Be(ws.Buffers.Length);
            processed.Buffers.ElementAt(0).Position.Should().Be(0);
        }

        [Fact]
        public async Task Processed_workspace_with_single_buffer_with_empty_id_generates_a_program_file()
        {
            var ws = new Workspace(
                buffers: new[]
                {
                    new Workspace.Buffer("", CodeSamples.SourceCodeProvider.ConsoleProgramSingleRegion, 0)
                });
            var processor = new BufferInliningTransformer();

            var processed = await processor.TransformAsync(ws);
            processed.Should().NotBeNull();
            processed.Files.Should().NotBeEmpty();
            var newCode = processed.Files.ElementAt(0).Text;
            newCode.Should().Contain(CodeSamples.SourceCodeProvider.ConsoleProgramSingleRegion);

        }
    }
}