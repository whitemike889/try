using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Recipes
{
    internal static class HttpResponseMessageExtensions
    {
        public static async Task<T> DeserializeAs<T>(this HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
