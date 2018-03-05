using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WorkspaceServer.Models.Execution;

namespace MLS.Agent.JsonContracts
{
    // todo : this is to be removed once migrated all to new protocol
    public class WorkspaceRequestConverter : JsonConverter
    {
        private static readonly HashSet<string> WorkspaceSignature;
        private static readonly HashSet<string> WorkspaceEnvelopeSignature;

        static WorkspaceRequestConverter()
        {
            WorkspaceSignature = new HashSet<string>(typeof(Workspace).GetConstructors()?.SelectMany(c => c.GetParameters()?.Select(p => p.Name)) ?? Enumerable.Empty<string>(),
                StringComparer.OrdinalIgnoreCase);
            WorkspaceEnvelopeSignature =
                new HashSet<string>(typeof(WorkspaceRequest).GetConstructors()?.SelectMany(c => c?.GetParameters().Select(p => p.Name)) ?? Enumerable.Empty<string>(),
                    StringComparer.OrdinalIgnoreCase);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var converter = serializer.Converters.FirstOrDefault(e => e.GetType() == GetType());
            RemoveConverter(converter, serializer);
            serializer.Serialize(writer, value);
            RestoreConverter(serializer, converter);
        }

       

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = serializer.Deserialize(reader) as JObject;

            var isWorkspace = obj?.Properties().All(p => WorkspaceSignature.Contains(p.Name)) == true;
            var isWorkspaceEnvelope = obj?.Properties().All(p => WorkspaceEnvelopeSignature.Contains(p.Name)) == true;
            var converter = serializer.Converters.FirstOrDefault(e => e.GetType() == GetType());
            if (isWorkspace)
            {
                RemoveConverter(converter, serializer);
                var ws = obj.ToObject<Workspace>();
                var ret = new WorkspaceRequest(ws);
                RestoreConverter(serializer, converter);
                return ret;
            }

            if (isWorkspaceEnvelope)
            {
                RemoveConverter(converter, serializer);
                var ret = obj.ToObject<WorkspaceRequest>(serializer);
                RestoreConverter(serializer, converter);
                return ret;
            }

            return null;
        }
        public override bool CanConvert(Type objectType)
        {
            var targeType = typeof(WorkspaceRequest);
            var response = targeType == objectType || objectType.IsSubclassOf(typeof(WorkspaceRequest));
            if (response)
                return true;

            return false;
        }

        private void RemoveConverter(JsonConverter jsonConverter, JsonSerializer serializer)
        {
            if (jsonConverter != null)
                serializer.Converters.Remove(jsonConverter);
        }
        private void RestoreConverter(JsonSerializer serializer, JsonConverter converter)
        {
            if (converter != null)
                serializer.Converters.Add(this);
        }
    }
}