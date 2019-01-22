using System;

namespace MLS.Repositories.Tests
{
    public class GitHubRepoLocatorTests : RepoLocatorTests
    {
        protected override IRepoLocator GetLocator()
        {
            return new GitHubRepoLocator();
        }
    }
}
