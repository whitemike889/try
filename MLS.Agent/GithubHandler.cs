using MLS.Repositories;
using System.CommandLine;
using System.Linq;
using System.Threading.Tasks;

namespace MLS.Agent
{
    public static class GithubHandler
    {
        public static async Task Handler(string repo, IConsole console, IRepoLocator locator)
        {
            var repos = (await locator.LocateRepo(repo)).ToArray();

            if (repos.Length == 0)
            {
                console.Out.WriteLine($"Didn't find any repos called `{repo}`");
            }
            else if (repos[0].Name == repo)
            {
                console.Out.WriteLine(GenerateCommandExample(repos[0].Name, repos[0].CloneUrl));

            }
            else
            {
                console.Out.WriteLine("Which of the following did you mean?");
                foreach (var instance in repos)
                {
                    console.Out.WriteLine($"\t{instance.Name}");
                }
            }

            string GenerateCommandExample(string name, string cloneUrl)
            {
                string text = $"Found repo `{name}`\n";
                text += $"To try `{name}`, cd to your desired directory and run the following command:\n\n";
                text += $"\tgit clone {cloneUrl} && dotnet try .";

                return text;
            }
        }
    }
}
