namespace Microsoft.DotNet.Try.Protocol.Execution
{
    public interface IRunResultFeature
    {
        string Name { get; }
        void Apply(FeatureContainer result);
    }
}
