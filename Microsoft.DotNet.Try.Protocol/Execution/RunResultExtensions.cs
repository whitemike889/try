namespace Microsoft.DotNet.Try.Protocol.Execution
{
    public static class RunResultExtensions
    {
        public static T GetFeature<T>(this FeatureContainer result) 
            where T : class, IRunResultFeature
        {
            if (result.Features.TryGetValue(typeof(T).Name, out var feature))
            {
                return feature as T;
            }
            else
            {
                return null;
            }
        }
    }
}