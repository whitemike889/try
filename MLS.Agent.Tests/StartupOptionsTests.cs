using System.IO;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using MLS.Agent.CommandLine;
using Xunit;

namespace MLS.Agent.Tests
{
    public class StartupOptionsTests
    {
        [Fact]
        public void When_root_directory_is_null_then_hosted_mode_is_true()
        {
            var options = new StartupOptions(rootDirectory: null);

            options.IsInHostedMode.Should().BeTrue();
        }

        [Fact]
        public void When_root_directory_is_set_then_hosted_mode_is_false()
        {
            var options = new StartupOptions(rootDirectory: new DirectoryInfo(Directory.GetCurrentDirectory()));

            options.IsInHostedMode.Should().BeFalse();
        }

        [Fact]
        public void When_Production_is_true_and_in_hosted_mode_then_EnvironmentName_is_production()
        {
            var options = new StartupOptions(production: true, rootDirectory: null);

            options.EnvironmentName.Should().Be(EnvironmentName.Production);
        }

        [Fact]
        public void When_Production_is_true_and_not_in_hosted_mode_then_EnvironmentName_is_production()
        {
            var options = new StartupOptions(production: true, rootDirectory: new DirectoryInfo(Directory.GetCurrentDirectory()));

            options.EnvironmentName.Should().Be(EnvironmentName.Production);
        }

        [Fact]
        public void When_not_in_hosted_mode_then_EnvironmentName_is_production()
        {
            var options = new StartupOptions(rootDirectory: new DirectoryInfo(Directory.GetCurrentDirectory()));

            options.EnvironmentName.Should().Be(EnvironmentName.Production);
        }
    }
}