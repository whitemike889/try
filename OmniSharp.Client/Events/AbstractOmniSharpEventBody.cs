namespace OmniSharp.Client.Events
{
    public abstract class AbstractOmniSharpEventBody : IOmniSharpEventBody
    {
        private readonly string name;

        protected AbstractOmniSharpEventBody()
        {
            name = GetType().Name;
        }

        string IOmniSharpEventBody.Name => name;
    }
}