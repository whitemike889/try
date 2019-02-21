using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using FluentAssertions;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace MLS.Agent.Tests
{
    public class CommandLineParserTests
    {
        private readonly ITestOutputHelper _output;
        private readonly TestConsole _console = new TestConsole();
        private StartupOptions _start_options;
        private readonly Parser _parser;
        private string _tryGitHub_repo;
        private DirectoryInfo _pack_packTarget;
        private string _install_packageName;
        private DirectoryInfo _install_packageSource;
        private DirectoryInfo _verify_rootDirectory;
        private bool _verify_compile;

        public CommandLineParserTests(ITestOutputHelper output)
        {
            _output = output;
            _parser = CommandLineParser.Create(
                start: (options, invocationContext) =>
                {
                    _start_options = options;
                },
                tryGithub: (repo, c) =>
                {
                    _tryGitHub_repo = repo;
                    return Task.CompletedTask;
                },
                pack: (packTarget, console) =>
                {
                    _pack_packTarget = packTarget;
                    return Task.CompletedTask;
                },
                install: (packageName, addSource, console) =>
                {
                    _install_packageName = packageName;
                    _install_packageSource = addSource;
                    return Task.CompletedTask;
                },
                verify: (rootDirectory, console, compile) =>
                {
                    _verify_rootDirectory = rootDirectory;
                    _verify_compile = compile;
                    return Task.FromResult(1);
                });
        }

        [Fact]
        public async Task Parse_empty_command_line_has_sane_defaults()
        {
            await _parser.InvokeAsync("hosted", _console);

            _start_options.Production.Should().BeFalse();
        }

        [Fact]
        public async Task Parse_production_mode_flag_switches_option_to_production()
        {
            await _parser.InvokeAsync("hosted --production", _console);

            _start_options.Production.Should().BeTrue();
        }

        [Fact]
        public async Task Parse_root_directory_with_a_valid_path_succeeds()
        {
            var path = TestAssets.SampleConsole.FullName;
            await _parser.InvokeAsync(new[] { path }, _console);
            _start_options.RootDirectory.FullName.Should().Be(path);
        }

        [Fact]
        public async Task Parse_empty_command_line_has_current_directory_as_root_directory()
        {
            await _parser.InvokeAsync("", _console);
            _start_options.RootDirectory.FullName.Should().Be(Directory.GetCurrentDirectory());
        }

        [Fact]
        public async Task Parse_root_directory_with_a_non_existing_path_fails()
        {
            await _parser.InvokeAsync("INVALIDPATH", _console);
            _start_options.Should().BeNull();
            _console.Error.ToString().Should().Match("*Directory does not exist: INVALIDPATH*");
        }

        [Fact]
        public async Task Parse_uri_workspace()
        {
            await _parser.InvokeAsync("--uri https://google.com/foo.md", _console);
            _start_options.Uri.Should().Be("https://google.com/foo.md");
        }

        [Fact]
        public async Task Parse_language_service_mode_flag_switches_option_to_language_service()
        {
            await _parser.InvokeAsync("hosted --language-service", _console);
            _start_options.IsLanguageService.Should().BeTrue();
        }

        [Fact]
        public void Parse_key_without_parameter_fails_the_parse()
        {
            _parser.Parse("hosted -k")
                   .Errors
                   .Should()
                   .Contain(e => e.Message == "Required argument missing for option: -k");

            _parser.Parse("hosted --key")
                   .Errors
                   .Should()
                   .Contain(e => e.Message == "Required argument missing for option: --key");
        }

        [Fact]
        public async Task Parse_key_with_parameter_succeeds()
        {
            await _parser.InvokeAsync("hosted -k abc123", _console);
            _start_options.Key.Should().Be("abc123");

            await _parser.InvokeAsync("hosted --key abc123", _console);
            _start_options.Key.Should().Be("abc123");
        }

        [Fact]
        public async Task AiKey_defaults_to_null()
        {
            await _parser.InvokeAsync("hosted", _console);
            _start_options.ApplicationInsightsKey.Should().BeNull();
        }

        [Fact]
        public void Parse_application_insights_key_without_parameter_fails_the_parse()
        {
            var result = _parser.Parse("hosted --ai-key");

            result.Errors.Should().Contain(e => e.Message == "Required argument missing for option: --ai-key");
        }

        [Fact]
        public async Task Parse_aiKey_with_parameter_succeeds()
        {
            await _parser.InvokeAsync("hosted --ai-key abc123", _console);
            _start_options.ApplicationInsightsKey.Should().Be("abc123");
        }

        [Fact]
        public async Task When_root_command_is_specified_then_agent_is_not_in_hosted_mode()
        {
            await _parser.InvokeAsync("", _console);
            _start_options.IsInHostedMode.Should().BeFalse();
        }

        [Fact]
        public async Task When_hosted_command_is_specified_then_agent_is_in_hosted_mode()
        {
            await _parser.InvokeAsync("hosted", _console);
            _start_options.IsInHostedMode.Should().BeTrue();
        }

        [Fact]
        public async Task GitHub_handler_not_run_if_argument_is_missing()
        {
            _tryGitHub_repo = "value";
            await _parser.InvokeAsync("github");
            _tryGitHub_repo.Should().Be("value");
        }

        [Fact]
        public async Task GitHub_handler_run_if_argument_is_present()
        {
            _tryGitHub_repo = "value";
            await _parser.InvokeAsync("github roslyn");
            _tryGitHub_repo.Should().Be("roslyn");
        }

        [Fact]
        public async Task Pack_not_run_if_argument_is_missing()
        {
            var console = new TestConsole();
            _pack_packTarget = null;
            await _parser.InvokeAsync("pack", console);
            console.Out.ToString().Should().Contain("pack <packTarget>");
            _pack_packTarget.Should().BeNull();
        }

        [Fact]
        public async Task Pack_parses_directory_info()
        {
            var console = new TestConsole();
            _pack_packTarget = null;
            var expected = Path.GetDirectoryName(typeof(PackageCommand).Assembly.Location);

            await _parser.InvokeAsync($"pack {expected}", console);
            _pack_packTarget.FullName.Should().Be(expected);
        }

        [Fact]
        public async Task Install_not_run_if_argument_is_missing()
        {
            var console = new TestConsole();
            _install_packageName = null;
            await _parser.InvokeAsync("install", console);
            console.Out.ToString().Should().Contain("install [options] <packageName>");
            _install_packageName.Should().BeNull();
        }

        [Fact]
        public async Task Install_parses_source_option()
        {
            var console = new TestConsole();
            _install_packageName = null;
            _install_packageSource = null;

            var expectedPackageSource = Path.GetDirectoryName(typeof(PackageCommand).Assembly.Location);

            await _parser.InvokeAsync($"install --add-source {expectedPackageSource} the-package", console);

            _install_packageName.Should().Be("the-package");
            _install_packageSource.FullName.Should().Be(expectedPackageSource);
        }

        [Fact]
        public async Task Verify_argument_specifies_root_directory()
        {
            var directory = Path.GetDirectoryName(typeof(VerifyCommand).Assembly.Location);
             await _parser.InvokeAsync($"verify {directory}");
            _verify_rootDirectory.FullName.Should().Be(directory);
        }

        [Fact]
        public async Task Verify_compile_option_defaults_to_false()
        {
            await _parser.InvokeAsync("verify");
            _verify_compile.Should().BeFalse();
        }

        [Fact]
        public async Task Verify_can_set_compile_to_true()
        {
            await _parser.InvokeAsync("verify --compile");
            _verify_compile.Should().BeTrue();
        }
    }
}