namespace MLS.Jupyter
{
    public interface IRequestHandlerStatus
    {
        void SetAsBusy();
        void SetAsIdle();
    }
}