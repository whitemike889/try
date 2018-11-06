using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace MLS.Protocol.Tests
{
    public class MarkdownStringTests

    {
        [Fact]
        public void CanDeserializeFromJson()
        {
            var source = "{\r\n  \"value\": \"Gets or sets the background color of the console.\",\r\n   \"isTrusted\": false\r\n    }";
            var ms = JsonConvert.DeserializeObject<MarkdownString>(source);
            ms.IsTrusted.Should().BeFalse();
            ms.Value.Should().Be("Gets or sets the background color of the console.");
        }

        [Fact]
        public void CanBeAssignedFromString()
        {
            var source = "Gets or sets the background color of the console.";
            MarkdownString ms = source;
            ms.IsTrusted.Should().BeFalse();
            ms.Value.Should().Be("Gets or sets the background color of the console.");
        }
    }
}
