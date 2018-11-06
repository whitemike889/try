using System.Linq;
using FluentAssertions;
using MLS.Protocol.SignatureHelp;
using Newtonsoft.Json;
using Xunit;

namespace MLS.Protocol.Tests
{
    public class SignatureHelpItemTests
    {
        [Theory]
        [InlineData("{\r\n      \"name\": \"Write\",\r\n      \"label\": \"void Console.Write(bool value)\",\r\n      \"documentation\": {\r\n        \"value\": \"Writes the text representation of the specified Boolean value to the standard output stream.\",\r\n        \"isTrusted\": false\r\n      },\r\n      \"parameters\": [\r\n        {\r\n          \"name\": \"value\",\r\n          \"label\": \"bool value\",\r\n          \"documentation\": {\r\n            \"value\": \"**value**: The value to write.\",\r\n            \"isTrusted\": false\r\n          }\r\n        }\r\n      ]\r\n    }")]
        [InlineData("{\r\n  \"name\": \"Write\",\r\n  \"label\": \"void Console.Write(bool value)\",\r\n  \"documentation\": \"Writes the text representation of the specified Boolean value to the standard output stream.\",\r\n  \"parameters\": [\r\n    {\r\n      \"name\": \"value\",\r\n      \"label\": \"bool value\",\r\n      \"documentation\": \"**value**: The value to write.\"\r\n    }\r\n  ]\r\n}")]
        public void CanDeserializeFromJson(string source)
        {

            var si = JsonConvert.DeserializeObject<SignatureHelpItem>(source);
            si.Documentation.Should().NotBeNull();
            si.Documentation.IsTrusted.Should().BeFalse();
            si.Documentation.Value.Should().Be("Writes the text representation of the specified Boolean value to the standard output stream.");

            si.Parameters.Should().NotBeNullOrEmpty();
            si.Parameters.First().Documentation.Value.Should().Be("**value**: The value to write.");
        }
    }
}