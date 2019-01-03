using FluentAssertions;
using MLS.Repositories;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MLS.Agent.Tests
{
    public class GitHubCommandTests
    {
        IRepoLocator _locator = new TestRepoLocator(new[]
            {
                new Repo("foo", "http://github.com/foo.git"),
                new Repo("bar", "http://github.com/bar.git")
            });

        class TestRepoLocator : IRepoLocator
        {
            private readonly IEnumerable<Repo> _repos;


            public TestRepoLocator(Repo[] repos)
            {
                _repos = repos;
            }

            public Task<IEnumerable<Repo>> LocateRepo(string repo)
            {
                return Task.FromResult(_repos);
            }
        }

        [Fact]
        public async Task It_reports_no_matches()
        {
            var console = new TestConsole();
            await Program.GithubHandler("foo", console, new TestRepoLocator(new Repo[] { }));
            console.Out.ToString().Replace("\r\n", "\n")
                .Should().Be("Didn't find any repos called `foo`\n");

        }

        [Fact]
        public async Task It_finds_the_requested_repo()
        {
            var console = new TestConsole();
            await Program.GithubHandler("foo", console, _locator);
            console.Out.ToString().Replace("\r\n", "\n").
                Should().Be("Found repo `foo`\nTo try `foo`, cd to your desired directory and run the following command:\n\n\tgit clone http://github.com/foo.git && dotnet try .\n");

        }

        [Fact]
        public async Task It_asks_for_disambiguation()
        {
            var console = new TestConsole();
            await Program.GithubHandler("something", console, _locator);
            console.Out.ToString().Replace("\r\n", "\n").Should().Be("Which of the following did you mean?\n\tfoo\n\tbar\n");
        }
    }
}
