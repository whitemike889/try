using System;
using FluentAssertions;
using System.Threading.Tasks;
using Clockwise;
using Pocket;
using Xunit;
using Xunit.Abstractions;
using WorkspaceServer.Packaging;
using System.IO;

namespace WorkspaceServer.Tests
{
    public class RebuildablePackageTests : IDisposable
    {
        private readonly CompositeDisposable disposables = new CompositeDisposable();

        public RebuildablePackageTests(ITestOutputHelper output)
        {
            disposables.Add(output.SubscribeToPocketLogger());
            disposables.Add(VirtualClock.Start());
        }

        public void Dispose() => disposables.Dispose();

        [Fact]
        public async Task If_a_new_file_is_added_and_clean_and_build_is_called_the_new_file_is_in_the_source_files()
        {
            var package = (RebuildablePackage) await Create.ConsoleWorkspaceCopy(isRebuildable: true);
            var commandLineArguments = await package.GetCommandLineArguments();
            var newFile = Path.Combine(package.Directory.FullName, "Sample.cs");
            commandLineArguments.SourceFiles.Should().NotContain(file => file.Path == newFile);

            File.WriteAllText(newFile, "//this is a new file");

            commandLineArguments = await package.GetCommandLineArguments();
            commandLineArguments.SourceFiles.Should().Contain(file => file.Path == newFile);
        }

        [Fact]
        public async Task If_the_project_file_is_changed_and_clean_and_build_is_called_the_compiler_arguments_are_changed()
        {
            var package = (RebuildablePackage)await Create.ConsoleWorkspaceCopy(isRebuildable: true);
            var commandLineArguments = await package.GetCommandLineArguments();
            commandLineArguments.MetadataReferences.Should().NotContain(reference => reference.Reference.Contains("microsoft.codeanalysis.csharp") && reference.Reference.Contains("2.8.2"));

            await new Dotnet(package.Directory).AddPackage("Microsoft.CodeAnalysis", "2.8.2");

            commandLineArguments = await package.GetCommandLineArguments();
            commandLineArguments.MetadataReferences.Should().Contain(reference => reference.Reference.Contains("microsoft.codeanalysis.csharp") && reference.Reference.Contains("2.8.2"));
        }

        [Fact]
        public async Task Gets_the_command_line_arguments_from_the_package()
        {
            var package = Create.EmptyWorkspace(isRebuildablePackage: true);
            await new Dotnet(package.Directory).New("console");
            var commandLineArguments = await package.GetCommandLineArguments();
            commandLineArguments.SourceFiles.Should().Contain(f => f.Path.Contains("Program.cs"));
        }
    }
}
