using System;
using System.Collections.Generic;
using MLS.Protocol.Execution;

namespace WorkspaceServer.WorkspaceFeatures
{
    public class UnitTestRun : IRunResultFeature
    {
        public UnitTestRun(IEnumerable<UnitTestResult> results)
        {
            Results = results ?? throw new ArgumentNullException(nameof(results)) ;
        }

        public IEnumerable<UnitTestResult> Results { get; }

        public void Apply(RunResult result)
        {
            result.AddProperty("testResults", Results);
        }
    }
}