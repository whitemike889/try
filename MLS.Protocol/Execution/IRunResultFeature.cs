namespace MLS.Protocol.Execution
{
    public interface IRunResultFeature
    {
        string Name { get; }
        void Apply(FeatureContainer result);
    }
}
