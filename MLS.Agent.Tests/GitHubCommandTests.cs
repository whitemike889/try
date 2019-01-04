using FluentAssertions;
using MLS.Repositories;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static MLS.Agent.GithubHandler;

namespace MLS.Agent.Tests
{
    public class GitHubCommandTests
    {
        IRepoLocator _locator = new Simulator();
        //IRepoLocator _locator = new TestRepoLocator(new[]
        //    {
        //        new Repo("foo", "http://github.com/foo.git"),
        //        new Repo("bar", "http://github.com/bar.git")
        //    });

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
            await Handler("foo", console, _locator);
            console.Out.ToString().Replace("\r\n", "\n")
                .Should().Be("Didn't find any repos called `foo`\n");

        }

        [Fact]
        public async Task It_finds_the_requested_repo()
        {
            var console = new TestConsole();
            await Handler("2660eaec-6af8-452d-b70d-41227d616cd9", console, _locator);
            console.Out.ToString().Replace("\r\n", "\n").
                Should().Be("Found repo `2660eaec-6af8-452d-b70d-41227d616cd`\nTo try `2660eaec-6af8-452d-b70d-41227d616cd, cd to your desired directory and run the following command:\n\n\tgit clone http://github.com/2660eaec-6af8-452d-b70d-41227d616cd.git && dotnet try .\n");

        }

        [Fact]
        public async Task It_asks_for_disambiguation()
        {
            var console = new TestConsole();
            await Handler("rchande/tribble", console, _locator);
            console.Out.ToString().Replace("\r\n", "\n").Should().Be("Which of the following did you mean?\n\t\n\tbar\n");
        }
    }
}
