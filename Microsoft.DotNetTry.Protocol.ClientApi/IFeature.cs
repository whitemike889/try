namespace Microsoft.DotNet.Try.Protocol.ClientApi
{
    public interface IFeature
    {
        string Name { get; }
        void Apply(FeatureContainer result);
    }
}