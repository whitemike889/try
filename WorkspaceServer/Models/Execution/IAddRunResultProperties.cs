namespace WorkspaceServer.Models.Execution
{
    public interface IAddRunResultProperties
    {
        void Augment(RunResult runResult, AddRunResultProperty addProperty);
    }
}
