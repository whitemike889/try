namespace MLS.Agent.CommandLine
{
    public class TryGitHubOptions
    {
        public TryGitHubOptions(string repo)
        {
            Repo = repo;
        }

        public string Repo { get; }
    }
}