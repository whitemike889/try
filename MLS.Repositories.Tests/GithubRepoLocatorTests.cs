using System;
using System.Collections.Generic;
using System.Text;

namespace MLS.Repositories.Tests
{
    public class GithubRepoLocatorTests : RepoLocatorTests
    {
        protected override IRepoLocator GetLocator()
        {
            return new GithubRepoLocator();
        }
    }
}
