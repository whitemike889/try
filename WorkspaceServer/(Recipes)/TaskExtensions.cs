using System;
using System.Threading.Tasks;

namespace Recipes
{
    internal static class TaskExtensions
    {
        public static async Task Timeout(
            this Task task,
            TimeSpan timeout)
        {
            if (await Task.WhenAny(
                    task,
                    Task.Delay(timeout)) != task)
            {
                throw new TimeoutException();
            }
        }
    }
}
