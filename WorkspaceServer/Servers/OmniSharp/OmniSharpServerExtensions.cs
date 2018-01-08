using System;
using System.IO;
using System.Reactive.Linq;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OmniSharp.Client;
using OmniSharp.Client.Commands;
using OmniSharp.Client.Events;
using Pocket;
using Recipes;
using static Pocket.Logger<MLS.Agent.Tools.OmniSharp>;

namespace WorkspaceServer.Servers.OmniSharp
{
    public static class OmniSharpServerExtensions
    {
        public static async Task ProjectLoaded(
            this OmniSharpServer omniSharpServer,
            TimeSpan? timeout = null)
        {
            using (var operation = Log.OnEnterAndConfirmOnExit())
            {
                await omniSharpServer.StandardOutput
                                     .AsOmniSharpMessages()
                                     .OfType<OmniSharpEventMessage<ProjectAdded>>()
                                     .FirstAsync()
                                     .Timeout(timeout ?? TimeSpan.FromSeconds(20));

                operation.Succeed();
            }
        }

        public static async Task<FileInfo> FindFile(this OmniSharpServer omnisharp, string name, TimeSpan? timeout = null) =>
            (await omnisharp.GetWorkspaceInformation(timeout))
            .Body
            .MSBuildSolution
            .Projects
            .Single()
            .SourceFiles
            .Single(f => f.Name == name);

        public static async Task<OmniSharpResponseMessage> SendCommand(
            this OmniSharpServer omnisharp,
            OmniSharpCommandMessage commandMessage,
            TimeSpan? timeout = null)
        {
            var json = commandMessage.ToJson(new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            omnisharp.StandardInput.WriteLine(json);

            var received = await omnisharp.StandardOutput
                                          .AsOmniSharpMessages()
                                          .OfType<OmniSharpResponseMessage>()
                                          .Where(m => m.Request_seq == commandMessage.Seq)
                                          .FirstAsync()
                                          .Timeout(timeout ?? TimeSpan.FromSeconds(10));

            return received;
        }

        public static async Task<OmniSharpResponseMessage<TResponse>> SendCommand<TCommand, TResponse>(
            this OmniSharpServer omnisharp,
            TimeSpan? timeout = null,
            int? seq = null)
            where TCommand : class, IOmniSharpCommandArguments, new() =>
            await omnisharp.SendCommand<TCommand, TResponse>(new TCommand(), timeout, seq);

        public static async Task<OmniSharpResponseMessage<TResponse>> SendCommand<TCommand, TResponse>(
            this OmniSharpServer omnisharp,
            TCommand command,
            TimeSpan? timeout = null,
            int? seq = null) where TCommand : class, IOmniSharpCommandArguments
        {
            seq = seq ?? omnisharp.NextSeq();

            var commandMessage = new OmniSharpCommandMessage<TCommand>(
                command,
                seq.Value);

            var received = await omnisharp.SendCommand(
                               commandMessage,
                               timeout);

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
            TimeSpan? timeout = null) =>
            server.SendCommand<CodeCheck, CodeCheckResponse>(new CodeCheck(file, buffer), timeout);

        public static Task<OmniSharpResponseMessage<EmitResponse>> Emit(
            this OmniSharpServer server,
            TimeSpan? timeout = null) =>
            server.SendCommand<Emit, EmitResponse>(new Emit(), timeout);

        public static Task<OmniSharpResponseMessage<WorkspaceInformationResponse>> GetWorkspaceInformation(
            this OmniSharpServer server,
            TimeSpan? timeout = null) =>
            server.SendCommand<WorkspaceInformation, WorkspaceInformationResponse>(timeout);

        public static Task UpdateBuffer(
            this OmniSharpServer server,
            FileInfo file,
            string newText,
            TimeSpan? timeout = null) =>
            server.SendCommand<UpdateBuffer, bool>(new UpdateBuffer(file, newText), timeout);
    }
}
