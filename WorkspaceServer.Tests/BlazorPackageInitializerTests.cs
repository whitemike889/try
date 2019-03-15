using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WorkspaceServer.Packaging;
using Xunit;

namespace WorkspaceServer.Tests
{
    public class BlazorPackageInitializerTests
    {
        [Fact]
        public async Task Requires_MLS_Blazor_directory()
        {
            var empty = Create.EmptyWorkspace();

            var initializer = new BlazorPackageInitializer(
                "blazor-test",
                new List<string>());

            Func<Task> initialize = async () =>
                await initializer.Initialize(empty.Directory);

            initialize.Should().Throw<ArgumentException>();

        }

        [Fact]
        public async Task Initializes_project_with_right_files()
        {
            var empty = Create.EmptyWorkspace();
            var dir = empty.Directory.CreateSubdirectory("MLS.Blazor");

            var name = "blazor-test";
            var initializer = new BlazorPackageInitializer(
                name,
                new List<string>());

            await initializer.Initialize(dir);

            var rootFiles = dir.GetFiles();
            rootFiles.Should().Contain(f => f.Name == "Program.cs");
            rootFiles.Should().Contain(f => f.Name == "Startup.cs");
            rootFiles.Should().Contain(f => f.Name == "Linker.xml");
            rootFiles.Should().Contain(f => f.Name == "App.cshtml");
            rootFiles.Should().Contain(f => f.Name == "MLS.Blazor.csproj");

            var pagesFiles = dir.GetFiles("Pages/*", SearchOption.AllDirectories);
            pagesFiles.Should().OnlyContain(
                f => f.Name == "Index.cshtml" || f.Name == "Index.cshtml.cs");

            var wwwrootFiles = dir.GetFiles("wwwroot/*", SearchOption.AllDirectories);
            wwwrootFiles.Should().OnlyContain(
                f => f.Name == "index.html" || f.Name == "interop.js");

            File.ReadAllText(Path.Combine(dir.FullName, "wwwroot", "index.html"))
                .Should().
                Contain($@"<base href=""/LocalCodeRunner/{name}/"" />");
        }

        [Fact]
        public async Task Calls_package_addition_funcs()
        {
            var empty = Create.EmptyWorkspace();
            var dir = empty.Directory.CreateSubdirectory("MLS.Blazor");

            bool called = false;

            var name = "blazor-test";
            var initializer = new BlazorPackageInitializer(
                name,
                new List<Func<Task>>()
                {
                    () => {
                        called = true;
                        return Task.CompletedTask;
                    }
                });

            await initializer.Initialize(dir);

            called.Should().Be(true);
        }
    }
}
