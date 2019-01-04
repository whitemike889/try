using System;
using System.Collections.Generic;
using System.Text;

namespace MLS.Repositories.Tests
{
    public class RealTests : RepoSearchSimulatorTests
    {
        protected override IRepoLocator GetLocator()
        {
            return new RepoLocator();
        }
    }
}
