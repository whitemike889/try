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

            var message = Serializer.DeserializeOmniSharpMessage(@event);

            message.Should().BeOfType<OmniSharpEventMessage<ProjectAdded>>();
            message.Seq.Should().Be(22);
            message.Type.Should().Be("event");
            message.As<OmniSharpEventMessage<ProjectAdded>>().Event.Should().Be("ProjectAdded");
        }

        [Fact]
        public void Unrecognized_event_types_are_deserialized()
        {
            var @event = @"{
  ""Event"": ""SomethingSomething"",
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

            var message = Serializer.DeserializeOmniSharpMessage(@event);

            message.Should().BeOfType<OmniSharpUnknownEventMessage>();
            message.Seq.Should().Be(22);
            message.Type.Should().Be("event");
            message.As<OmniSharpEventMessage>().Event.Should().Be("SomethingSomething");
        }
    }
}
