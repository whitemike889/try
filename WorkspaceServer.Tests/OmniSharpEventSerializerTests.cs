using System;
using FluentAssertions;
using OmniSharp.Client;
using OmniSharp.Client.Events;
using Xunit;

namespace WorkspaceServer.Tests
{
    public class OmniSharpEventSerializerTests
    {
        [Fact]
        public void Specific_event_types_are_selected_from_event_type_field()
        {
            var @event = @"{
  ""Event"": ""ProjectAdded"",
  ""Body"": {
    ""MsBuildProject"": {
      ""ProjectGuid"": ""00000000-0000-0000-0000-000000000000"",
      ""Path"": ""C:\\temp\\MyConsoleApp\\MyConsoleApp.csproj"",
      ""AssemblyName"": ""MyConsoleApp"",
      ""TargetPath"": ""C:\\temp\\MyConsoleApp\\bin\\Debug\\netcoreapp2.0\\MyConsoleApp.dll"",
      ""TargetFramework"": "".NETCoreApp,Version=v2.0"",
      ""SourceFiles"": [
        ""C:\\temp\\MyConsoleApp\\Program.cs"",
        ""C:\\Users\\josequ.REDMOND\\AppData\\Local\\Temp\\.NETCoreApp,Version=v2.0.AssemblyAttributes.cs"",
        ""C:\\temp\\MyConsoleApp\\obj\\Debug\\netcoreapp2.0\\MyConsoleApp.AssemblyInfo.cs""
      ],
      ""TargetFrameworks"": [
        {
          ""Name"": "".NETCoreApp"",
          ""FriendlyName"": "".NETCoreApp"",
          ""ShortName"": ""netcoreapp2.0""
        }
      ],
      ""OutputPath"": ""bin\\Debug\\netcoreapp2.0\\"",
      ""IsExe"": true,
      ""IsUnityProject"": false
    }
  },
  ""Seq"": 22,
  ""Type"": ""event""
}";

            Serializer.DeserializeEvent(@event).Should().BeOfType<ProjectAdded>();
        }

        [Fact]
        public void When_an_event_name_is_unrecognized_then_deserialization_throws()
        {
             var @event = @"{
  ""Event"": ""ThisEventDoesNotExist"",
  ""Body"": {
  },
  ""Seq"": 22,
  ""Type"": ""event""
}";

            Action deserialize = () => Serializer.DeserializeEvent(@event);

            deserialize
                .ShouldThrow<EventSerializationException>()
                .Which
                .Message
                .Should()
                .Contain("ThisEventDoesNotExist");
        }
    }
}
