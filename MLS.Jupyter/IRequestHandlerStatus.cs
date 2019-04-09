namespace Microsoft.DotNet.Try.Jupyter
{
    public interface IRequestHandlerStatus
    {
        void SetAsBusy();
        void SetAsIdle();
    }
}