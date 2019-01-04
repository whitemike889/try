using System;
using System.Collections.Generic;
using System.Text;

namespace MLS.Repositories.Tests
{
    public class RealTests : IRepoLocatorTests
    {
        protected override IRepoLocator GetLocator()
        {
            return new RepoLocator();
        }
    }
}
