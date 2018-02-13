using System;
using Newtonsoft.Json;

namespace Recipes
{
    internal static class JsonSerializationExtensions
    {
        public static string ToJson(this object source, JsonSerializerSettings settings = null) =>
            JsonConvert.SerializeObject(source, settings);

        public static T FromJsonTo<T>(this string json, JsonSerializerSettings settings = null) =>
            JsonConvert.DeserializeObject<T>(json, settings);

        public static object FromJsonTo(this string json, Type type, JsonSerializerSettings settings = null) =>
            JsonConvert.DeserializeObject(json, type, settings);
    }
}
