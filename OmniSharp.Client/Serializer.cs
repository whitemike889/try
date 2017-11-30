using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OmniSharp.Client.Events;

namespace OmniSharp.Client
{
    public static class Serializer
    {
        public static OmnisharpEvent DeserializeEvent(string json)
        {
            var @event = JsonConvert.DeserializeObject<EventEnvelope>(json);
            if (@event.Event == "ProjectAdded")
            {
                return @event.Body.ToObject<ProjectAdded>();
            }
            return null;
        }

        private class EventEnvelope
        {
            public string Event;
            public JObject Body;
            public int Seq;
        }
    }
}