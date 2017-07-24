using Newtonsoft.Json;

namespace Recipes
{
    internal static class JsonSerializationExtensions
    {
        public static string ToJson(this object source) =>
            JsonConvert.SerializeObject(source);
    }
}