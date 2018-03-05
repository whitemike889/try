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
        private readonly HashSet<string> _workspaceSignature;
        private readonly HashSet<string> _workspaceEnvelopeSignature;

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
                serializer.Converters.Remove(this);
                var ret = obj.ToObject<WorkspaceRequest>(serializer);
                serializer.Converters.Add(this);
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
