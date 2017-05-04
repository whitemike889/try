using System;
using Newtonsoft.Json;

namespace WorkspaceServer.Tests._Recipes_
{
    internal static class JsonSerializationExtensions
    {
        public static string ToJson(this object source) =>
            JsonConvert.SerializeObject(source);
    }
}