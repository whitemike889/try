using System;
using System.IO;
using System.Reactive.Linq;
using System.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OmniSharp.Client;
using OmniSharp.Client.Commands;
using Recipes;

namespace WorkspaceServer.Servers.OmniSharp
{
    public static class OmniSharpServerExtensions
    {
        public static async Task<FileInfo> FindFile(this OmniSharpServer omniSharp, string name, CancellationToken? cancellationToken = null)
        {
            if (omniSharp == null)
            {
                throw new ArgumentNullException(nameof(omniSharp));
            }

            await omniSharp.WorkspaceReady(cancellationToken);

            return (await omniSharp.GetWorkspaceInformation(cancellationToken))
                   .Body
                   .MSBuildSolution
                   .Projects
                   .Single()
                   .SourceFiles
                   .Single(f => f.Name == name);
        }

        public static async Task<OmniSharpResponseMessage> SendCommand(
            this OmniSharpServer omniSharp,
            OmniSharpCommandMessage commandMessage,
            CancellationToken? cancellationToken = null)
        {
            if (omniSharp == null)
            {
                throw new ArgumentNullException(nameof(omniSharp));
            }

            await omniSharp.WorkspaceReady(cancellationToken);

            var json = commandMessage.ToJson(new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            omniSharp.StandardInput.WriteLine(json);

            var received = await omniSharp.StandardOutput
                                          .AsOmniSharpMessages()
                                          .OfType<OmniSharpResponseMessage>()
                                          .Where(m => m.Request_seq == commandMessage.Seq)
                                          .FirstAsync()
                                          .ToTask(cancellationToken);

            return received;
        }

        public static async Task<OmniSharpResponseMessage<TResponse>> SendCommand<TCommand, TResponse>(
            this OmniSharpServer omniSharp,
            CancellationToken? cancellationToken = null,
            int? seq = null)
            where TCommand : class, IOmniSharpCommandArguments, new() =>
            await omniSharp.SendCommand<TCommand, TResponse>(new TCommand(), cancellationToken, seq);

        public static async Task<OmniSharpResponseMessage<TResponse>> SendCommand<TCommand, TResponse>(
            this OmniSharpServer omniSharp,
            TCommand command,
            CancellationToken? cancellationToken = null,
            int? seq = null) where TCommand : class, IOmniSharpCommandArguments
        {
            seq = seq ?? omniSharp.NextSeq();

            var commandMessage = new OmniSharpCommandMessage<TCommand>(
                command,
                seq.Value);

            var received = await omniSharp.SendCommand(
                               commandMessage,
                               cancellationToken);

            switch (received)
            {
                case OmniSharpResponseMessage<TResponse> expected:
                    return expected;

                case OmniSharpUnknownResponseMessage unknown:
                    return new OmniSharpResponseMessage<TResponse>(
                        unknown.Body.ToObject<TResponse>(),
                        unknown.Success,
                        unknown.Message,
                        unknown.Command,
                        unknown.Seq,
                        unknown.Request_seq);

                default:
                    throw new OmniSharpMessageSerializationException("");
            }
        }

        public static Task<OmniSharpResponseMessage<CodeCheckResponse>> CodeCheck(
            this OmniSharpServer server,
            FileInfo file = null,
            string buffer = null,
            CancellationToken? cancellationToken = null) =>
            server.SendCommand<CodeCheck, CodeCheckResponse>(new CodeCheck(file, buffer), cancellationToken);

        public static Task<OmniSharpResponseMessage<EmitResponse>> Emit(
            this OmniSharpServer server,
            CancellationToken? cancellationToken = null) =>
            server.SendCommand<Emit, EmitResponse>(new Emit(), cancellationToken);

        public static Task<OmniSharpResponseMessage<WorkspaceInformationResponse>> GetWorkspaceInformation(
            this OmniSharpServer server,
            CancellationToken? cancellationToken = null) =>
            server.SendCommand<WorkspaceInformation, WorkspaceInformationResponse>(cancellationToken);

        public static Task UpdateBuffer(
            this OmniSharpServer server,
            FileInfo file,
            string newText,
            CancellationToken? cancellationToken = null) =>
            server.SendCommand<UpdateBuffer, bool>(new UpdateBuffer(file, newText), cancellationToken);
    }
}
