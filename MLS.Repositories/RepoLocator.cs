using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MLS.Repositories
{
    public class RepoLocator
    {


        public static async Task<IEnumerable<string>> LocateRepo(string repo)
        {
            var req = new SearchRepositoriesRequest(repo);

            var client = new GitHubClient(new ProductHeaderValue("github-try-demo"));

            var result = await client.Search.SearchRepo(req);
            return result.Items.Select(i => i.FullName);
        }
    }
}
