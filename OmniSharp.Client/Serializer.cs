using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Newtonsoft.Json.Linq;
using OmniSharp.Client.Commands;
using OmniSharp.Client.Events;
using Recipes;

namespace OmniSharp.Client
{
    public static class Serializer
    {
        private static readonly Dictionary<string, Func<MessageEnvelope, OmniSharpEventMessage>> eventDeserializers = new Dictionary<string, Func<MessageEnvelope, OmniSharpEventMessage>>
        {
            [nameof(ProjectAdded)] = envelope => envelope.ToEvent<ProjectAdded>(),
            [nameof(error)] = envelope => envelope.ToEvent<error>(),
            [nameof(log)] = envelope => envelope.ToEvent<log>(),
            [nameof(started)] = envelope => envelope.ToEvent<started>(),
        };

        private static readonly Dictionary<string, Func<MessageEnvelope, OmniSharpResponseMessage>> responseDeserializers =
            new Dictionary<string, Func<MessageEnvelope, OmniSharpResponseMessage>>
            {
                [CommandNames.CodeCheck] = envelope => envelope.ToCommandResponse<CodeCheckResponse>(),
                [CommandNames.Emit] = envelope => envelope.ToCommandResponse<EmitResponse>(),
                [CommandNames.UpdateBuffer] = envelope => envelope.ToCommandResponse<bool?>()
            };

        public static OmniSharpMessage DeserializeOmniSharpMessage(string json)
        {
            var envelope = json.FromJsonTo<MessageEnvelope>();

            if (envelope.Type == "event")
            {
                if (eventDeserializers.TryGetValue(envelope.Event, out var deserialize
                    ))
                {
                    return deserialize(envelope);
                }
                else
                {
                    return new OmniSharpUnknownEventMessage(
                        envelope.Body,
                        envelope.Event,
                        envelope.Seq);
                }
            }

            if (envelope.Type == "response")
            {
                if (responseDeserializers.TryGetValue(envelope.Command, out var deserialize))
                {
                    return deserialize(envelope);
                }
                else
                {
                    return new OmniSharpUnknownResponseMessage(
                        envelope.Body,
                        envelope.Success,
                        envelope.Message,
                        envelope.Command,
                        envelope.Seq,
                        envelope.Request_seq);
                }
            }

            throw new OmniSharpMessageSerializationException($"Unknown message type: {json}");
        }

        public static IObservable<OmniSharpMessage> AsOmniSharpMessages(this IObservable<string> jsonEvents) =>
            jsonEvents.Select(DeserializeOmniSharpMessage);

        private class MessageEnvelope
        {
            public string Event = null;
            public string Type = null;
            public JToken Body = null;
            public int Seq = 0;
            public bool Success = false;
            public int Request_seq = 0;
            public string Command = null;
            public string Message = null;

            public OmniSharpEventMessage<T> ToEvent<T>()
                where T : class, IOmniSharpEventBody =>
                new OmniSharpEventMessage<T>(
                    Event,
                    Body.ToObject<T>(),
                    Seq);

            public OmniSharpResponseMessage ToCommandResponse<T>() =>
                new OmniSharpResponseMessage<T>(
                    Body.ToObject<T>(),
                    Success,
                    Message,
                    Command,
                    Seq,
                    Request_seq);
        }
    }
}
