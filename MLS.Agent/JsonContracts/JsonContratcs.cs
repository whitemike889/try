using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WorkspaceServer.Models.Execution;

namespace MLS.Agent.JsonContracts
{
    public static class JsonContratcs
    {
        public static void Setup()
        {
            var settings = JsonConvert.DefaultSettings?.Invoke() ?? new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter> { new WorkspaceRequestConverter() }
            };

            JsonConvert.DefaultSettings = () => settings;
        }
    }

    public class WorkspaceRequestConverter : JsonConverter
    {
        private readonly HashSet<string> _workspaceSignature;
        private readonly HashSet<string> _workspaceEnvelopeSignature;
        private JsonConverter _wsec;

        public WorkspaceRequestConverter()
        {
            _workspaceSignature = new HashSet<string>(typeof(Workspace).GetConstructors()?.SelectMany(c => c.GetParameters()?.Select(p => p.Name)) ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            _workspaceEnvelopeSignature = new HashSet<string>(typeof(WorkspaceRequest).GetConstructors()?.SelectMany(c => c?.GetParameters().Select(p => p.Name)) ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);}

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (_wsec == null)
            {
                _wsec = JsonConvert.DefaultSettings?.Invoke()?.Converters.FirstOrDefault(c => c.GetType() == this.GetType());
            }

            var obj = serializer.Deserialize(reader) as JObject;

            var isWorkspace = obj?.Properties().All(p => _workspaceSignature.Contains(p.Name)) == true;
            var isWorkspaceEnvelope = obj?.Properties().All(p => _workspaceEnvelopeSignature.Contains(p.Name)) == true;

            if (isWorkspace)
            {
                var ws = obj.ToObject<Workspace>();
                return new WorkspaceRequest(ws);
            }

            if (isWorkspaceEnvelope)
            {
                serializer.Converters.Remove(_wsec);
                var ret = obj.ToObject<WorkspaceRequest>(serializer);
                serializer.Converters.Add(_wsec);
                return ret;
            }

            return null;
        }

        public override bool CanConvert(Type objectType)
        {
            var targeType = typeof(WorkspaceRequest);
            var response =  targeType == objectType || objectType.IsSubclassOf(typeof(WorkspaceRequest));
            if (response)
            {
                return true;
            }

            return false;
        }
    }
}
