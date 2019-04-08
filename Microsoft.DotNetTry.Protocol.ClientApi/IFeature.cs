namespace Microsoft.DotNetTry.Protocol.ClientApi
{
    public interface IFeature
    {
        string Name { get; }
        void Apply(FeatureContainer result);
    }
}