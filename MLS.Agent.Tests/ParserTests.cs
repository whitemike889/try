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
        [Fact]
        public void TestEmpty()
        {
            var options = CommandLineOptions.Parse(new string[] { });
            options.IsProduction.Should().Be(false);
            options.HelpRequested.Should().Be(false);
            options.HelpText.Should().Be(null);
        }

        [Fact]
        public void TestProduction()
        {
            var options = CommandLineOptions.Parse(new string[] { "--production" });
            options.IsProduction.Should().Be(true);
            options.HelpRequested.Should().Be(false);
            options.HelpText.Should().Be(null);
        }

        [Fact]
        public void TestHelp()
        {
            {
                var options = CommandLineOptions.Parse(new string[] { "--help" });
                options.HelpRequested.Should().Be(true);
                options.HelpText.Should().NotBe(null);
            }
            {
                var options = CommandLineOptions.Parse(new string[] { "-h" });
                options.HelpRequested.Should().Be(true);
                options.HelpText.Should().NotBe(null);
            }
        }

        [Fact]
        public void TestKey()
        {
            {
                var options = CommandLineOptions.Parse(new string[] { "-k" });
                options.WasSuccess.Should().BeFalse();
            }
            {
                var options = CommandLineOptions.Parse(new string[] { "--key" });
                options.WasSuccess.Should().BeFalse();
            }

            {
                var options = CommandLineOptions.Parse(new string[] { "-k", "abc123" });
                Console.WriteLine(options.HelpText);
                options.WasSuccess.Should().BeTrue();
                options.Key.Should().Be("abc123");
            }
            {
                var options = CommandLineOptions.Parse(new string[] { "--key", "abc123" });
                options.WasSuccess.Should().BeTrue();
                options.Key.Should().Be("abc123");
            }
        }
    }
}
