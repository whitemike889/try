namespace Microsoft.DotNet.Try.Protocol
{
    public interface IRunResultFeature
    {
        string Name { get; }
        void Apply(FeatureContainer result);
    }
}
