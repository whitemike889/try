using System;
using System.IO;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using MLS.Agent.CommandLine;
using Xunit;

namespace MLS.Agent.Tests.CommandLine
{
    public class StartupOptionsTests
    {
        [Fact]
        public void When_root_directory_is_null_then_mode_is_Hosted()
        {
            var options = new StartupOptions(dir: null);

            options.Mode.Should().Be(StartupMode.Hosted);
        }

        [Fact]
        public void When_root_directory_is_set_then_mode_is_Try()
        {
            var options = new StartupOptions(dir: new DirectoryInfo(Directory.GetCurrentDirectory()));

            options.Mode.Should().Be(StartupMode.Try);
        }

        [Fact]
        public void When_Production_is_true_and_in_hosted_mode_then_EnvironmentName_is_production()
        {
            var options = new StartupOptions(production: true, dir: null);

            options.EnvironmentName.Should().Be(EnvironmentName.Production);
        }

        [Fact]
        public void When_Production_is_true_and_not_in_hosted_mode_then_EnvironmentName_is_production()
        {
            var options = new StartupOptions(production: true, dir: new DirectoryInfo(Directory.GetCurrentDirectory()));

            options.EnvironmentName.Should().Be(EnvironmentName.Production);
        }

        [Fact]
        public void When_not_in_hosted_mode_then_EnvironmentName_is_production()
        {
            var options = new StartupOptions(dir: new DirectoryInfo(Directory.GetCurrentDirectory()));

            options.EnvironmentName.Should().Be(EnvironmentName.Production);
        }
    }
}