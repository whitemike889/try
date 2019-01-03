using System.Collections.Generic;
using System.Threading.Tasks;

namespace MLS.Repositories
{
    public interface IRepoLocator
    {
        Task<IEnumerable<Repo>> LocateRepo(string repo);
    }

    public class Repo
    {
        public Repo(string name, string cloneUrl)
        {
            Name = name;
            CloneUrl = cloneUrl;
        }

        public string Name { get; }
        public string CloneUrl { get; }
    }
}
