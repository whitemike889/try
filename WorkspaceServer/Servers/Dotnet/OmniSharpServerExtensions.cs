using System;
using System.Collections.Generic;
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
using WorkspaceServer.Models.SingatureHelp;
using SignatureHelpRequest = OmniSharp.Client.Commands.SignatureHelpRequest;

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
            Budget budget = null)
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
                                          .ToTask()
                                          .CancelIfExceeds(budget ?? new Budget());
            budget?.RecordEntry();
            return received;
        }

        public static async Task<OmniSharpResponseMessage<TResponse>> SendCommand<TCommand, TResponse>(
            this OmniSharpServer omniSharp,
            Budget budget = null,
            int? seq = null)
            where TCommand : class, IOmniSharpCommandArguments, new() =>
            await omniSharp.SendCommand<TCommand, TResponse>(new TCommand(), budget, seq);

        public static async Task<OmniSharpResponseMessage<TResponse>> SendCommand<TCommand, TResponse>(
            this OmniSharpServer omniSharp,
            TCommand command,
            Budget budget = null,
            int? seq = null) where TCommand : class, IOmniSharpCommandArguments
        {
            seq = seq ?? omniSharp.NextSeq();

            var commandMessage = new OmniSharpCommandMessage<TCommand>(
                command,
                seq.Value);

            var received = await omniSharp.SendCommand(
                               commandMessage,
                               budget);
            
            OmniSharpResponseMessage<TResponse> response;
            switch (received)
            {
                case OmniSharpResponseMessage<TResponse> expected:
                    response = expected;
                    break;
                case OmniSharpUnknownResponseMessage unknown:
                    response = new OmniSharpResponseMessage<TResponse>(
                        unknown.Body.ToObject<TResponse>(),
                        unknown.Success,
                        unknown.Message,
                        unknown.Command,
                        unknown.Seq,
                        unknown.Request_seq);
                    break;
                default:
                    throw new OmniSharpMessageSerializationException($"Unrecognized: {received.Message}");
            }

            return response;
        }

        public static Task<OmniSharpResponseMessage<CodeCheckResponse>> CodeCheck(
            this OmniSharpServer server,
            FileInfo file = null,
            string buffer = null,
            Budget budget = null) =>
            server.SendCommand<CodeCheck, CodeCheckResponse>(new CodeCheck(file, buffer), budget);

        public static Task<OmniSharpResponseMessage<EmitResponse>> Emit(
            this OmniSharpServer server,
            IEnumerable<InstrumentationRegionMap> instrumentationRegions = null,
            bool includeInstrumentation = false,
            Budget budget = null) =>
            server.SendCommand<Emit, EmitResponse>(new Emit(includeInstrumentation, instrumentationRegions), budget);

        public static Task<OmniSharpResponseMessage<WorkspaceInformationResponse>> GetWorkspaceInformation(
            this OmniSharpServer server,
            Budget budget = null) =>
            server.SendCommand<WorkspaceInformation, WorkspaceInformationResponse>(budget);

        public static Task UpdateBuffer(
            this OmniSharpServer server,
            FileInfo file,
            string newText,
            Budget budget = null) =>
            server.SendCommand<UpdateBuffer, bool?>(new UpdateBuffer(file, newText), budget);

        public static async Task<SignatureHelpResponse> GetSignatureHelp(this OmniSharpServer server, FileInfo fileName, string code, int line, int column, Budget budget = null)
        {
            // as omnisharp deserialisation does a -1 in the contract we add 1
            // look at https://github.com/OmniSharp/omnisharp-roslyn/blob/e18913e887144119c41d60f1842e49f8e9bfcf72/src/OmniSharp.Abstractions/Models/Request.cs
            var command = new SignatureHelpRequest(fileName,code, line + 1, column + 1);
 
            var received = await server.SendCommand<SignatureHelpRequest, SignatureHelpResponse>(
                command,
                budget);

            return received.Body;

        }

        public static async Task<AutoCompleteResponse[]> GetCompletionList(this OmniSharpServer server, FileInfo fileName, string code, string wordToComplete, int line, int column, Budget budget = null)
        {
            // as omnisharp deserialisation does a -1 in the contract we add 1
            // look at https://github.com/OmniSharp/omnisharp-roslyn/blob/e18913e887144119c41d60f1842e49f8e9bfcf72/src/OmniSharp.Abstractions/Models/Request.cs
            var command = new AutoCompleteRequest(fileName, code, line + 1, column + 1)
            {
                WordToComplete = wordToComplete,
                WantKind = true,
                WantDocumentationForEveryCompletionResult = true,
                WantReturnType = true
            };

            var received = await server.SendCommand<AutoCompleteRequest, AutoCompleteResponse[]>(
                command,
                budget);

            return received.Body;

        }

    }
}
