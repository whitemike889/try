using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLS.Repositories.Tests
{
    public class SimulatorTests : RepoSearchSimulatorTests
    {
        protected override IRepoLocator GetLocator()
        {
            return new Simulator();
        }
    }
}
