namespace MLS.Agent.Markdown
{
    public interface IDirectoryAccessor
    {
        bool FileExists(string filePath);
        string ReadAllText(string filePath);
    }
}