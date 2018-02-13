using System.Threading.Tasks;

namespace Recipes
{
    internal static class TaskExtensions
    {
        /// <summary>
        /// Provides a way to specify the intention to fire and forget a task and suppress the compiler warning that results from unawaited tasks.
        /// </summary>
        internal static void DontAwait(this Task task)
        {
        }
    }
}