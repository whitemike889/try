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
            options.production.Should().Be(false);
            options.helpRequested.Should().Be(false);
            options.helpText.Should().Be(null);
        }

        [Fact]
        public void TestProduction()
        {
            var options = CommandLineOptions.Parse(new string[] { "--production" });
            options.production.Should().Be(true);
            options.helpRequested.Should().Be(false);
            options.helpText.Should().Be(null);
        }

        [Fact]
        public void TestHelp()
        {
            {
                var options = CommandLineOptions.Parse(new string[] { "--help" });
                options.helpRequested.Should().Be(true);
                options.helpText.Should().NotBe(null);
            }
            {
                var options = CommandLineOptions.Parse(new string[] { "-h" });
                options.helpRequested.Should().Be(true);
                options.helpText.Should().NotBe(null);
            }
        }

        [Fact]
        public void TestKey()
        {
            {
                var options = CommandLineOptions.Parse(new string[] { "-k" });
                options.wasSuccess.Should().BeFalse();
            }
            {
                var options = CommandLineOptions.Parse(new string[] { "--key" });
                options.wasSuccess.Should().BeFalse();
            }

            {
                var options = CommandLineOptions.Parse(new string[] { "-k", "abc123" });
                Console.WriteLine(options.helpText);
                options.wasSuccess.Should().BeTrue();
                options.key.Should().Be("abc123");
            }
            {
                var options = CommandLineOptions.Parse(new string[] { "--key", "abc123" });
                options.wasSuccess.Should().BeTrue();
                options.key.Should().Be("abc123");
            }
        }
    }
}
