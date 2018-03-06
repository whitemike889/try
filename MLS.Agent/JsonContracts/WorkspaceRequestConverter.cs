using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WorkspaceServer.Models.Execution;

namespace MLS.Agent.JsonContracts
{
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
            RemoveConverter(serializer, converter);
            try
            {
                serializer.Serialize(writer, value);
            }
            finally
            {
                RestoreConverter(serializer, converter);
            }
        }



        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = serializer.Deserialize(reader) as JObject;
            WorkspaceRequest workspaceRequest = null;
            var isWorkspace = obj?.Properties().All(p => WorkspaceSignature.Contains(p.Name)) == true;
            var isWorkspaceEnvelope = obj?.Properties().All(p => WorkspaceEnvelopeSignature.Contains(p.Name)) == true;
            var converter = serializer.Converters.FirstOrDefault(e => e.GetType() == GetType());
            if (isWorkspace)
            {
                RemoveConverter(serializer, converter);
                try
                {
                    var ws = obj.ToObject<Workspace>();
                    workspaceRequest = new WorkspaceRequest(ws);
                }
                finally
                {
                    RestoreConverter(serializer, converter);
                }
            }
            else if (isWorkspaceEnvelope)
            {
                RemoveConverter(serializer, converter);
                try
                {
                    workspaceRequest = obj.ToObject<WorkspaceRequest>(serializer);
                }
                finally
                {
                    RestoreConverter(serializer, converter);
                }
            }

            return workspaceRequest;
        }
        public override bool CanConvert(Type objectType)
        {
            var targeType = typeof(WorkspaceRequest);
            var response = targeType == objectType || objectType.IsSubclassOf(typeof(WorkspaceRequest));
            return response;
        }

        private void RemoveConverter(JsonSerializer serializer, JsonConverter converter)
        {
            if (converter != null)
                serializer.Converters.Remove(converter);
        }

        private void RestoreConverter(JsonSerializer serializer, JsonConverter converter)
        {
            if (converter != null)
                serializer.Converters.Add(this);
        }
    }
}
