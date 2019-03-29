using System;
using System.Collections.Generic;
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
                    var transient = new Dictionary<string, object> { { "display_id", Guid.NewGuid().ToString() } };

                    var jObject = (JObject)delivery.Command.Request.Content;
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

                    // display data
                    var data = new DisplayData
                    {
                        Data = new JObject()
                        {
                            { "text/plain", string.Join("\n",result.Output) }
                        },
                        Transient = transient
                    };

                    // execute result
                    var executeResultData = new ExecuteResult
                    {
                        Data = new JObject()
                        {
                            { "text/plain", string.Join("\n",result.Output) }
                        },
                        Transient = transient
                    };

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

                        //if (!executeRequest.Silent)
                        //{
                        //    // send on io
                        //    var displayData = delivery.Command.Builder.CreateMessage(MessageTypeValues.DisplayData, data, delivery.Command.Request.Header);
                        //    delivery.Command.IoPubChannel.Send(displayData);
                        //}
                    }
                    else
                    {
                        var errorContent = new Error
                        {
                            EName = string.IsNullOrWhiteSpace(result.Exception) ? "Compiler Error" : "Unhandled Exception",
                            EValue = string.Join("\n", result.Output),
                            Traceback = new List<string>()
                        };

                        //  reply Error
                        var executeReplyPayload = new ExecuteReplyError(errorContent)
                        {
                            ExecutionCount = _executionCount
                        };

                        // send to server
                        var executeReply = delivery.Command.Builder.CreateMessage(MessageTypeValues.ExecuteReply, executeReplyPayload, delivery.Command.Request.Header);
                        executeReply.Identifiers = delivery.Command.Request.Identifiers;
                        delivery.Command.ServerChannel.Send(executeReply);

                        if (!executeRequest.Silent)
                        {
                            // send on io
                            var error = delivery.Command.Builder.CreateMessage(MessageTypeValues.Error, errorContent, delivery.Command.Request.Header);
                            delivery.Command.IoPubChannel.Send(error);

                            // send on stderr
                            var stdErr = new StdErrStream
                            {
                                Text = errorContent.EValue
                            };
                            var stream = delivery.Command.Builder.CreateMessage(MessageTypeValues.Stream, stdErr, delivery.Command.Request.Header);
                            delivery.Command.IoPubChannel.Send(stream);
                        }
                    }

                    if (!executeRequest.Silent)
                    {
                        var executeResult = delivery.Command.Builder.CreateMessage(MessageTypeValues.ExecuteResult, executeResultData, delivery.Command.Request.Header);
                        delivery.Command.IoPubChannel.Send(executeResult);
                    }

                    break;
            }

            return delivery.Complete();
        }
    }
}