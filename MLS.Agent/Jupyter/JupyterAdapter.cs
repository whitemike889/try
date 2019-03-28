using System;
using MLS.Jupyter;
using MLS.Jupyter.Protocol;
using WorkspaceServer.Servers.Roslyn;
using Pocket;
using static Pocket.Logger<MLS.Agent.Jupyter.JupyterAdapter>;

namespace MLS.Agent.Jupyter
{
    public class JupyterAdapter : IObserver<JupyterRequestContext>
    {
        private readonly Shell _jupyterShell;
        private readonly RoslynWorkspaceServer _workspaceServer;

        public JupyterAdapter(Shell jupyterShell, RoslynWorkspaceServer workspaceServer)
        {
            _jupyterShell = jupyterShell ?? throw new ArgumentNullException(nameof(jupyterShell));
            _workspaceServer = workspaceServer ?? throw new ArgumentNullException(nameof(workspaceServer));

            _jupyterShell.Subscribe(this);
        }

        void IObserver<JupyterRequestContext>.OnCompleted()
        {
          
        }

        void IObserver<JupyterRequestContext>.OnError(Exception error)
        {
            Log.Error(error);
        }

        void IObserver<JupyterRequestContext>.OnNext(JupyterRequestContext context)
        {
            switch (context.Request.Header.MessageType)
            {
                case MessageTypeValues.ExecuteRequest:
                ProcessExecuteRequest(context);
                    break;
            }
        }

        private void ProcessExecuteRequest(JupyterRequestContext context)
        {
            using (Log.OnEnterAndExit())
            {
                context.RequestHandlerStatus.SetAsBusy();

                context.RequestHandlerStatus.SetAsIdle();
            }
        }
    }
}