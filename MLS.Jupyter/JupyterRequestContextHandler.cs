using System;
using System.Linq;
using System.Threading.Tasks;
using Clockwise;
using MLS.Jupyter.Protocol;
using MLS.Protocol;
using MLS.Protocol.Execution;
using Newtonsoft.Json.Linq;
using WorkspaceServer;
using WorkspaceServer.Servers.Roslyn;

namespace MLS.Jupyter
{
    public class JupyterRequestContextHandler : ICommandHandler<JupyterRequestContext>
    {
        private readonly PackageRegistry _packageRegistry;
        private int _executionCount;

        public JupyterRequestContextHandler(PackageRegistry packageRegistry)
        {
            _packageRegistry = packageRegistry ?? throw new ArgumentNullException(nameof(packageRegistry));
        }

        public async Task<ICommandDeliveryResult> Handle(
            ICommandDelivery<JupyterRequestContext> delivery)
        {
            switch (delivery.Command.Request.Header.MessageType)
            {
                case MessageTypeValues.ExecuteRequest:
                    var jObject = (JObject) delivery.Command.Request.Content;
                    var executeRequest = jObject.ToObject<ExecuteRequest>();

                    var code = executeRequest.Code;

                    var package = await _packageRegistry.Get("console");

                    var workspace = new Workspace(
                        files: new[]
                               {
                                   new Workspace.File("Program.cs", code),
                               },
                        workspaceType: package.Name);

                    var workspaceRequest = new WorkspaceRequest(workspace);

                    var server = new RoslynWorkspaceServer(package);

                    var result = await server.Run(workspaceRequest);

                    if (!executeRequest.Silent)
                    {
                        _executionCount++;

                        var executeInput = delivery.Command.Builder.CreateMessage(
                            MessageTypeValues.ExecuteInput,
                            new ExecuteInput
                            {
                                Code = code,
                                ExecutionCount = _executionCount
                            },
                            delivery.Command.Request.Header);

                        delivery.Command.IoPubChannel.Send(executeInput);
                    }

                    if (result.Succeeded && 
                        result.Exception == null)
                    {

                        // reply ok
                        var executeReplyPayload = new ExecuteReplyOk
                        {
                            ExecutionCount = _executionCount
                        };

                        // send to server
                        var executeReply = delivery.Command.Builder.CreateMessage(MessageTypeValues.ExecuteReply, executeReplyPayload, delivery.Command.Request.Header);
                        executeReply.Identifiers = delivery.Command.Request.Identifiers;
                        delivery.Command.ServerChannel.Send(executeReply);

                        if (!executeRequest.Silent)
                        {
                            // display data
                            var data = new DisplayData
                            {
                                Data = new JObject()
                                {
                                    { "text/plain", string.Join("\n",result.Output) },
                                   
                                }
                            };

                            var displayData = delivery.Command.Builder.CreateMessage(MessageTypeValues.DisplayData, data, delivery.Command.Request.Header);
                            delivery.Command.IoPubChannel.Send(displayData);
                        }
                    }
                    else
                    {
                        var errorPayload = new ExecuteError
                        {
                            ExecutionCount = _executionCount,
                            EName = string.IsNullOrWhiteSpace(result.Exception) ? "Compiler Error" : "Unhandled Exception",
                            EValue = string.Join("\n", result.Output)
                        };
                       
                        // send on io
                        var error = delivery.Command.Builder.CreateMessage(MessageTypeValues.Error, errorPayload, delivery.Command.Request.Header);
                        delivery.Command.IoPubChannel.Send(error);

                        //  reply Error
                        var executeReplyPayload = new ExecuteReplyError(errorPayload);

                        // send to server
                        var executeReply = delivery.Command.Builder.CreateMessage(MessageTypeValues.ExecuteReply, executeReplyPayload, delivery.Command.Request.Header);
                        executeReply.Identifiers = delivery.Command.Request.Identifiers;
                        delivery.Command.ServerChannel.Send(executeReply);
                    }



                    break;
            }

            return delivery.Complete();
        }
    }
}