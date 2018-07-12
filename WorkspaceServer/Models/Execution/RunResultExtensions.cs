using System;

namespace WorkspaceServer.Models.Execution
{
    public static class RunResultExtensions
    {
        public static T GetFeature<T>(this RunResult result) 
            where T : class, IRunResultFeature => 
            result.Features.TryGetValue(typeof(T), out var feature)
                ? feature as T
                : throw new InvalidOperationException($"The feature is not enabled: {typeof(T)}");
    }
}