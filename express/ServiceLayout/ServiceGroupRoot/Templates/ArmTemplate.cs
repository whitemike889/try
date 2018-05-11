using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace express
{
    public static class ArmTemplate
    {
        static public Dictionary<string, string> AsDictionary()
        {
            var assembly = typeof(express.ArmTemplate).Assembly;
            var stream = assembly.GetManifestResourceStream("express.template.json");
            using (var reader = new StreamReader(stream))
            {
                var armTemplate = JObject.Parse(reader.ReadToEnd());

                return armTemplate.SelectTokens("$.resources[?(@.name=='trydotnet-westus')].resources[?(@.type=='config' && @.name=='connectionstrings')].properties")
                    .Values<JProperty>()
                    .ToDictionary(t => t.Name, t => t.Value["value"].Value<string>());
            }
        }
    }
}
