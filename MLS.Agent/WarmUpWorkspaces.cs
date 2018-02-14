using System;
using System.Threading;
using System.Threading.Tasks;
using Pocket;
using WorkspaceServer;
using static Pocket.Logger<MLS.Agent.WarmUpWorkspaces>;

namespace MLS.Agent
{
    public class WarmUpWorkspaces : HostedService, IDisposable
    {
        private readonly WorkspaceServerRegistry workspaceServerRegistry;
        private Thread thread;

        public WarmUpWorkspaces(WorkspaceServerRegistry workspaceServerRegistry)
        {
            this.workspaceServerRegistry = workspaceServerRegistry ??
                                           throw new ArgumentNullException(nameof(workspaceServerRegistry));
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            thread = new Thread(() =>
            {
                try
                {
                    workspaceServerRegistry.StartAllServers().Wait(cancellationToken);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex);
                }
            });

            thread.Start();
        }

        public void Dispose()
        {
        }
    }
}
