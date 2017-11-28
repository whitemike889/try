using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using Pocket;
using Recipes;
using WorkspaceServer.Models.Completion;
using Xunit;
using Xunit.Abstractions;
using MLS.Agent;

namespace MLS.Agent.Tests
{
    public class CommandLineParserTests
    {
        private void VerifySuccessfulAndNoHelpText(CommandLineOptions options)
        {
            options.WasSuccess.Should().BeTrue();
            options.HelpRequested.Should().BeFalse();
            options.HelpText.Should().BeNull();
        }

        [Fact]
        public void Parse_empty_command_line_has_sane_defaults()
        {
            var options = CommandLineOptions.Parse(new string[] { });
            VerifySuccessfulAndNoHelpText(options);
            options.IsProduction.Should().BeFalse();
        }

        [Fact]
        public void Parse_production_mode_flag_switches_option_to_production()
        {
            var options = CommandLineOptions.Parse(new string[] { "--production" });
            VerifySuccessfulAndNoHelpText(options);
            options.IsProduction.Should().BeTrue();
        }

        [Fact]
        public void Parse_help_flag()
        {
            {
                var options = CommandLineOptions.Parse(new string[] { "--help" });
                options.HelpRequested.Should().BeTrue();
                options.HelpText.Should().NotBeNull();
            }
            {
                var options = CommandLineOptions.Parse(new string[] { "-h" });
                options.HelpRequested.Should().BeTrue();
                options.HelpText.Should().NotBeNull();
            }
        }

        [Fact]
        public void Parse_key_without_parameter_fails_the_parse()
        {
            var options = CommandLineOptions.Parse(new string[] { "-k" });
            options.WasSuccess.Should().BeFalse();

            options = CommandLineOptions.Parse(new string[] { "--key" });
            options.WasSuccess.Should().BeFalse();
        }

        [Fact]
        public void Parse_key_with_parameter_succeeds()
        {
            var options = CommandLineOptions.Parse(new string[] { "-k", "abc123" });
            VerifySuccessfulAndNoHelpText(options);
            options.Key.Should().Be("abc123");

            options = CommandLineOptions.Parse(new string[] { "--key", "abc123" });
            VerifySuccessfulAndNoHelpText(options);
            options.Key.Should().Be("abc123");
        }
    }
}
