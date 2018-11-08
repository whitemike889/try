using System;
using System.Linq;
using FluentAssertions;
using MLS.Project.Extensions;
using MLS.Protocol.Execution;
using Xunit;

namespace MLS.Protocol.Tests
{
    public class Given_a_Workspace
    {
        [Fact]
        public void I_can_extracts_viewPorts_when_files_declare_region()
        {
            var ws = new Workspace(files: new[]
            {
                new Workspace.File("Program.cs", CodeSamples.SourceCodeProvider.ConsoleProgramSingleRegion)
            });

            var viewPorts = ws.ExtractViewPorts();
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

            Action extraction = () => ws.ExtractViewPorts().ToList();
            extraction.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void ViewPort_ids_must_be_uinique_inside_the_workspace()
        {
            var ws = new Workspace(files: new[]
            {
                new Workspace.File("ProgramA.cs", CodeSamples.SourceCodeProvider.ConsoleProgramSingleRegion),
                new Workspace.File("ProgramB.cs", CodeSamples.SourceCodeProvider.ConsoleProgramSingleRegion)
            });

            Action extraction = () => ws.ExtractViewPorts();
            extraction.Should().NotThrow<InvalidOperationException>();
        }
        
        [Fact]
        public void ViewPort_extraction_fails_with_null_workspace()
        {
            Action extraction = () => ((Workspace)null).ExtractViewPorts().ToList();
            extraction.Should().Throw<ArgumentNullException>();
        }

    }
}