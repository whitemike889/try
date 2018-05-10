using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Recipes
{
    internal static class HttpResponseMessageExtensions
    {
        public static async Task<T> DeserializeAs<T>(this HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            PropertyNamesAreCamelCase(JObject.Parse(json));
            return JsonConvert.DeserializeObject<T>(json);
        }

        private static void PropertyNamesAreCamelCase(JObject source)
        {
            if (source == null)
            {
                return;
            }

            foreach (var property in source.Properties())
            {
                property.Name.Should().MatchRegex(@"^[a-z].*", "property names should be camel case");
                if (property.Value.Type == JTokenType.Object)
                {
                    PropertyNamesAreCamelCase(source);
                }
                else if (property.Value.Type == JTokenType.Array)
                {
                    foreach (var element in property.Value.Value<JArray>())
                    {
                        if (element.Type == JTokenType.Object)
                        {
                            PropertyNamesAreCamelCase(element as JObject);
                        }
                    }
                }
            }
        }
    }
}
