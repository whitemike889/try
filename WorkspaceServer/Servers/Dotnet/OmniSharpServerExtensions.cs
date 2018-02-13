using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Clockwise;
using Newtonsoft.Json;
using OmniSharp.Client;
using OmniSharp.Client.Commands;
using Recipes;

namespace WorkspaceServer.Servers.Dotnet
{
    public static class OmniSharpServerExtensions
    {
        public static async Task<FileInfo> FindFile(this OmniSharpServer omniSharp, string name, TimeBudget budget = null)
        {
            if (omniSharp == null)
            {
                throw new ArgumentNullException(nameof(omniSharp));
            }

            await omniSharp.WorkspaceReady(budget);

            return (await omniSharp.GetWorkspaceInformation(budget))
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
            TimeBudget budget = null)
        {
            if (omniSharp == null)
            {
                throw new ArgumentNullException(nameof(omniSharp));
            }

            await omniSharp.WorkspaceReady(budget);

            var json = commandMessage.ToJson(new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            await omniSharp.Send(json);

            var received = await omniSharp.StandardOutput
                                          .AsOmniSharpMessages()
                                          .OfType<OmniSharpResponseMessage>()
                                          .Where(m => m.Request_seq == commandMessage.Seq)
                                          .FirstAsync()
                                          .ToTask(budget);

            return received;
        }

        public static async Task<OmniSharpResponseMessage<TResponse>> SendCommand<TCommand, TResponse>(
            this OmniSharpServer omniSharp,
            TimeBudget budget = null,
            int? seq = null)
            where TCommand : class, IOmniSharpCommandArguments, new() =>
            await omniSharp.SendCommand<TCommand, TResponse>(new TCommand(), budget, seq);

        public static async Task<OmniSharpResponseMessage<TResponse>> SendCommand<TCommand, TResponse>(
            this OmniSharpServer omniSharp,
            TCommand command,
            TimeBudget budget = null,
            int? seq = null) where TCommand : class, IOmniSharpCommandArguments
        {
            seq = seq ?? omniSharp.NextSeq();

            var commandMessage = new OmniSharpCommandMessage<TCommand>(
                command,
                seq.Value);

            var received = await omniSharp.SendCommand(
                               commandMessage,
                               budget);

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
            TimeBudget budget = null) =>
            server.SendCommand<CodeCheck, CodeCheckResponse>(new CodeCheck(file, buffer), budget);

        public static Task<OmniSharpResponseMessage<EmitResponse>> Emit(
            this OmniSharpServer server,
            TimeBudget budget = null) =>
            server.SendCommand<Emit, EmitResponse>(new Emit(), budget);

        public static Task<OmniSharpResponseMessage<WorkspaceInformationResponse>> GetWorkspaceInformation(
            this OmniSharpServer server,
            TimeBudget budget = null) =>
            server.SendCommand<WorkspaceInformation, WorkspaceInformationResponse>(budget);

        public static Task UpdateBuffer(
            this OmniSharpServer server,
            FileInfo file,
            string newText,
            TimeBudget budget = null) =>
            server.SendCommand<UpdateBuffer, bool>(new UpdateBuffer(file, newText), budget);
    }
}
