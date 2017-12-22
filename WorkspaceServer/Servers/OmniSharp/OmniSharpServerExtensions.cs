using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using OmniSharp.Client;
using OmniSharp.Client.Commands;
using OmniSharp.Client.Events;
using OmniSharp.Client.Responses;
using Recipes;

namespace WorkspaceServer.Servers.OmniSharp
{
    public static class OmniSharpServerExtensions
    {
        public static async Task ProjectLoaded(
            this OmniSharpServer omniSharpServer) =>
            await omniSharpServer.StandardOutput
                                 .AsOmniSharpMessages()
                                 .OfType<OmniSharpEventMessage<ProjectAdded>>()
                                 .FirstAsync()
                                 .Timeout(TimeSpan.FromSeconds(10));

        public static async Task<OmniSharpResponseMessage<TResponse>> SendCommand<TCommand, TResponse>(
            this OmniSharpServer omnisharp,
            TimeSpan? timeout = null,
            int? seq = null)
            where TResponse : class
            where TCommand : class, IOmniSharpCommandBody, new() =>
            await omnisharp.SendCommand<TCommand, TResponse>(new TCommand(), timeout, seq);

        public static async Task<OmniSharpResponseMessage<TResponse>> SendCommand<TCommand, TResponse>(
            this OmniSharpServer omnisharp,
            TCommand command,
            TimeSpan? timeout = null,
            int? seq = null)
            where TResponse : class
            where TCommand : class, IOmniSharpCommandBody
        {
            seq = seq ?? omnisharp.NextSeq();

            var commandMessage = new OmniSharpCommandMessage<TCommand>(command, seq.Value);

            omnisharp.StandardInput.WriteLine(commandMessage.ToJson());

            var received = await omnisharp.StandardOutput
                                          .AsOmniSharpMessages()
                                          .OfType<OmniSharpResponseMessage>()
                                          .Where(m => m.Request_seq == seq.Value)
                                          .FirstAsync().Timeout(timeout ?? TimeSpan.FromSeconds(10));

            switch (received)
            {
                case OmniSharpResponseMessage<TResponse> expected:
                    return expected;

                case OmniSharpUnknownResponseMessage unknown:
                    return new OmniSharpResponseMessage<TResponse>(
                        unknown.Body?.ToObject<TResponse>(),
                        unknown.Success,
                        unknown.Message,
                        unknown.Command,
                        unknown.Seq,
                        unknown.Request_seq);

                default:
                    throw new OmniSharpMessageSerializationException("");
            }
        }
    }
}
