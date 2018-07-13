using System;
using System.Collections.Generic;
using FluentAssertions;
using MLS.Agent.JsonContracts;
using Newtonsoft.Json;
using WorkspaceServer.Models;
using WorkspaceServer.Models.Execution;
using Xunit;

namespace MLS.Agent.Tests
{
    public class WorkspaceRequestTests
    {
        public WorkspaceRequestTests()
        {
            var settings = JsonConvert.DefaultSettings?.Invoke() ?? new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter> { new WorkspaceRequestConverter() }
            };

            JsonConvert.DefaultSettings = () => settings;
        }

        [Fact]
        public void can_augment_workspace_to_workpaceRequest()
        {
            var json = JsonConvert.SerializeObject(new
            {
                workspaceType = "console",
                buffers = new[] { new { id = "testId", content = "no code", position = 0 } }
            });

            var request = JsonConvert.DeserializeObject<WorkspaceRequest>(json);
            request.Should().NotBeNull();
        }

        [Fact]
        public void webrequest_must_have_verb()
        {
            var action = new Action(() =>
            {
                var wr = new HttpRequest(@"/handler", string.Empty);
            });
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void webrequest_must_have_relative_url()
        {
            var action = new Action(() =>
            {
                var wr = new HttpRequest(@"http://www.microsoft.com", "post");
            });
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void When_ActiveBufferId_is_not_specified_and_there_is_only_one_buffer_then_it_returns_that_buffers_id()
        {
            var request = new WorkspaceRequest(
                new Workspace(
                    buffers: new[]
                    {
                        new Workspace.Buffer("the.only.buffer.cs", "its content", 123)
                    }));

            request.ActiveBufferId.Should().Be("the.only.buffer.cs");
        }
    }
}
