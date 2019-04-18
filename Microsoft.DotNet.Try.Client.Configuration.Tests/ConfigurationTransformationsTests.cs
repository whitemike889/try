using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.DotNet.Try.Client.Configuration.Extensions;
using Xunit;

namespace Microsoft.DotNet.Try.Client.Configuration.Tests
{
    public class ConfigurationTransformationsTests
    {
        private readonly ClientConfiguration _sampleConfig;

        public ConfigurationTransformationsTests()
        {
            _sampleConfig = new ClientConfiguration(
                "A5F0CCD0-FE6F-4E5A-9E18-2CFA508F8423",
                new RequestDescriptors(new RequestDescriptor("/clientConfiguration"))
                {
                    Configuration = new RequestDescriptor("/clientConfiguration"),
                    Completion = new RequestDescriptor("/workspace/completion", properties: new[] { new RequestDescriptorProperty("completionProvider") }),
                    AcceptCompletion = new RequestDescriptor("/workspace/acceptCompletionItem", properties: new[] { new RequestDescriptorProperty("listId"), new RequestDescriptorProperty("index") }),
                    LoadFromGist = new RequestDescriptor("/workspace/fromgist/{gistId}/{commitHash?}", templated: true, method: "GET", properties: new[] { new RequestDescriptorProperty("workspaceType"), new RequestDescriptorProperty("extractBuffers") }),
                    Diagnostics = new RequestDescriptor("/workspace/diagnostics"),
                    SignatureHelp = new RequestDescriptor("/workspace/signatureHelp"),
                    Run = new RequestDescriptor("/workspace/run"),
                    Snippet = new RequestDescriptor("/snippet", method: "GET", properties: new[] { new RequestDescriptorProperty("from") })
                },15000, "ai-write-key", true);
        }

        [Fact]
        public void Generates_url_from_templated_api()
        {
            var apiLink = new RequestDescriptor("/url/with/{symbol1}", templated: true);
            var fullUrl = apiLink.BuildFullUri(new Dictionary<string, object> { { "symbol1", "nonOptional" } });
            fullUrl.Should().Be("/url/with/nonOptional");
        }

        [Fact]
        public void Generates_url_with_uriencoded_values()
        {
            var apiLink = new RequestDescriptor("/url/with/{symbol1}", templated: true, properties: new[] { new RequestDescriptorProperty("parameter1"), });
            var fullUrl = apiLink.BuildFullUri(new Dictionary<string, object> { { "symbol1", "https://this.needs.encoding?right=true" }, { "parameter1", "and this one as well!!" } });
            fullUrl.Should().Be("/url/with/https%3a%2f%2fthis.needs.encoding%3fright%3dtrue?parameter1=and+this+one+as+well!!");
        }

        [Fact]
        public void Generates_url_with_query()
        {
            var apiLink = new RequestDescriptor("/url",
                properties: new[]
                {
                    new RequestDescriptorProperty("queryOne"),
                    new RequestDescriptorProperty("queryTwo"),
                });

            var fullUrl = apiLink.BuildFullUri(new Dictionary<string, object> { { "queryOne", "string" }, { "queryTwo", true } });
            fullUrl.Should().Be("/url?queryOne=string&queryTwo=True");
        }

        [Fact]
        public void Generates_url_with_query_when_href_contains_query()
        {
            var apiLink = new RequestDescriptor("/url?alreadyHere=yes",
                properties: new[]
                {
                    new RequestDescriptorProperty("queryOne"),
                    new RequestDescriptorProperty("queryTwo"),
                });

            var fullUrl = apiLink.BuildFullUri(new Dictionary<string, object> { { "queryOne", "string" }, { "queryTwo", true } });
            fullUrl.Should().Be("/url?alreadyHere=yes&queryOne=string&queryTwo=True");
        }

        [Theory]
        [InlineData("POST")]
        [InlineData("GET")]
        [InlineData("DELETE")]
        [InlineData("PUT")]
        [InlineData("HEAD")]
        [InlineData("OPTIONS")]
        [InlineData("TRACE")]
        public void Generates_request_with_method(string method)
        {
            var apiLink = new RequestDescriptor("/url", method: method);

            var request = apiLink.BuildRequest();
            request.Method.ToString().Should().Be(method);
        }

        [Fact]
        public void Generates_request_with_headers()
        {
            var request = _sampleConfig.BuildRunRequest("", "http://some.url");

            request.Headers.Should().Contain(h => h.Key == "Timeout" && h.Value.Count() == 1);
            request.Headers.Should().Contain(h => h.Key == "ClientConfigurationVersionId" && h.Value.Count() == 1);
        }
    }
}
