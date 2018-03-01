namespace WorkspaceServer.Models.Execution
{
    public static class RunResultExtensions
    {
        public static T GetFeature<T>(this RunResult result)
            where T : class => 
            result.Features.TryGetValue(typeof(T), out var feature)
                ? feature as T
                : null;
    }
}