using System;
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
                        
                    }
                    else
                    {

                    }



                    break;
            }

            return delivery.Complete();
        }
    }
}