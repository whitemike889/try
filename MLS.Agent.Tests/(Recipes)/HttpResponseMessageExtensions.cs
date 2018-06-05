using System.Collections.Generic;
using System.Linq;
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
            if (!string.IsNullOrWhiteSpace(json))
            {
                PropertyNamesAreCamelCase(JToken.Parse(json));
            }
            return JsonConvert.DeserializeObject<T>(json);
        }

        private static void PropertyNamesAreCamelCase(JToken source)
        {
            switch (source)
            {
                case JObject o:
                    PropertyNamesAreCamelCase(o);
                    break;
            }
        }

        private static void PropertyNamesAreCamelCase(JObject source)
        {
            if (source == null || !source.HasValues)
            {
                return;
            }

            var toCheck = new Queue<JObject>();

            toCheck.Enqueue(source);

            while (toCheck.Count > 0)
            {
                var current = toCheck.Dequeue();
                if (current == null || !source.HasValues)
                {
                   continue;
                }

                var properties = current.Properties().ToList();
                foreach (var property in properties)
                {
                    property.Name.Should().MatchRegex(@"^(([a-z])|(\W*))(.*)", "property names should be camel case");
                }

                foreach (var property in properties)
                {
                    switch (property.Value.Type)
                    {
                        case JTokenType.Object:
                            toCheck.Enqueue(property.Value.Value<JObject>());
                            break;
                        case JTokenType.Array:
                            foreach (var element in property.Value.Value<JArray>())
                            {
                                if (element.Type == JTokenType.Object)
                                {
                                    toCheck.Enqueue(element as JObject);
                                }
                            }
                            break;
                    }
                }

            }
        }
    }
}
