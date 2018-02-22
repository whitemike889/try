using System.Linq;
using FluentAssertions;
using Newtonsoft.Json;
using WorkspaceServer.Models.Execution;
using Xunit;

namespace WorkspaceServer.Tests
{
    public class WorkspaceRunRequestTest
    {
        [Fact]
        public void Can_parse_simple_code_source_request()
        {
            var request = JsonConvert.DeserializeObject<Workspace>(@"{ source : ""code""}");
            request.Buffers.Should().NotBeNullOrEmpty();
            request.SourceFiles.Should().NotBeNullOrEmpty();
            request.WorkspaceType.Should().Be("script");

        }

        [Fact]
        public void Can_parse_buffer_request()
        {
            var request = JsonConvert.DeserializeObject<Workspace>(@"{ buffer : ""code"", bufferId:""test"", position: 12}");
            request.Buffers.Should().NotBeNullOrEmpty();
            request.SourceFiles.Should().NotBeNullOrEmpty();
            request.WorkspaceType.Should().Be("script");
            request.Buffers.FirstOrDefault(b => b.Id == "test").Should().NotBeNull();
        }

        [Fact]
        public void Can_parse_workspace_without_files()
        {
            var request = JsonConvert.DeserializeObject<Workspace>(@"{ workspaceType: ""console"", buffers: [{content: ""code"", id:""test"", position: 12}] }");
            request.Buffers.Should().NotBeNullOrEmpty();
            request.SourceFiles.Should().BeNullOrEmpty();
            request.WorkspaceType.Should().Be("console");
            request.Buffers.FirstOrDefault(b => b.Id == "test").Should().NotBeNull();
        }

        [Fact]
        public void Can_parse_workspace_with_files()
        {
            var request = JsonConvert.DeserializeObject<Workspace>(@"{ workspaceType: ""console"", buffers: [{content: ""code"", id:""test"", position: 12}], files:[{name: ""filedOne.cs"", text:""some value""}] }");
            request.Buffers.Should().NotBeNullOrEmpty();
            request.SourceFiles.Should().NotBeNullOrEmpty();
            request.WorkspaceType.Should().Be("console");
            request.Buffers.FirstOrDefault(b => b.Id == "test").Should().NotBeNull();
            request.SourceFiles.FirstOrDefault(b => b.Name == "filedOne.cs").Should().NotBeNull();
        }

        [Fact]
        public void Can_parse_workspace_with_usings()
        {
            var request = JsonConvert.DeserializeObject<Workspace>(@"{ usings: [""using System1;"", ""using System2;""], workspaceType: ""console"", buffers: [{content: ""code"", id:""test"", position: 12}], files:[{name: ""filedOne.cs"", text:""some value""}] }");
            request.Buffers.Should().NotBeNullOrEmpty();
            request.SourceFiles.Should().NotBeNullOrEmpty();
            request.WorkspaceType.Should().Be("console");
            request.Buffers.FirstOrDefault(b => b.Id == "test").Should().NotBeNull();
            request.SourceFiles.FirstOrDefault(b => b.Name == "filedOne.cs").Should().NotBeNull();
            request.Usings.ShouldBeEquivalentTo(new []{ "using System1;", "using System2;"});
        }
    }
}