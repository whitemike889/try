using System;
using System.Collections.Generic;
using MLS.Protocol.Execution;

namespace WorkspaceServer.Features
{
    public class UnitTestRun : IRunResultFeature
    {
        public UnitTestRun(IEnumerable<UnitTestResult> results)
        {
            Results = results ?? throw new ArgumentNullException(nameof(results)) ;
        }

        public IEnumerable<UnitTestResult> Results { get; }

        public string Name => nameof(UnitTestRun);

        public void Apply(FeatureContainer result)
        {
            result.AddProperty("testResults", Results);
        }
    }
}